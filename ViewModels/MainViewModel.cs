using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HubLink.Models;
using HubLink.Services;

namespace HubLink.ViewModels;

public partial class MainViewModel(ShipmentClient client, Func<string, Task<Stream?>> openExport) : ObservableObject
{
    private Shipment[] _shipments = [];
    private string? _snapshot;

    public string[] StatusFilters { get; } = ["All", .. Enum.GetNames<ShipmentStatus>()];
    public string[] Scenarios { get; } = ["Live", "normal", "empty", "bad-request", "server-error", "invalid-json", "delay"];
    public int[] Timeouts { get; } = [5, 15, 30];

    [ObservableProperty] private string _address = "http://127.0.0.1:5080";
    [ObservableProperty] private string _branch = "BR-001";
    [ObservableProperty] private string _scenario = "Live";
    [ObservableProperty] private int _timeoutSeconds = 15;
    [ObservableProperty] private string _selectedStatus = "All";
    [ObservableProperty] private bool _isLoading;
    [ObservableProperty] private bool _isExporting;
    [ObservableProperty] private bool _retrievalFailed;
    [ObservableProperty] private string _retrievalMessage = "Select Refresh shipments to load the branch.";
    [ObservableProperty] private string _exportMessage = "";
    [ObservableProperty] private Shipment[] _visibleShipments = [];
    [ObservableProperty] private string _snapshotLabel = "No branch loaded";

    public int Total => _shipments.Length;
    public int Pending => Count(ShipmentStatus.Pending);
    public int InTransit => Count(ShipmentStatus.InTransit);
    public int Delivered => Count(ShipmentStatus.Delivered);
    public int Exceptions => Count(ShipmentStatus.Exception);
    public string VisibleCount => $"Showing {VisibleShipments.Length} of {Total}";
    public bool CanEditSettings => !IsLoading && !IsExporting;

    private int Count(ShipmentStatus status) => _shipments.Count(row => row.Status == status);

    partial void OnSelectedStatusChanged(string value) => ApplyFilter();
    partial void OnIsLoadingChanged(bool value) => UpdateActions();
    partial void OnIsExportingChanged(bool value) => UpdateActions();

    private void UpdateActions()
    {
        OnPropertyChanged(nameof(CanEditSettings));
        RefreshCommand.NotifyCanExecuteChanged();
        ExportCommand.NotifyCanExecuteChanged();
    }

    private void ApplyFilter()
    {
        VisibleShipments = SelectedStatus == "All"
            ? _shipments
            : _shipments.Where(row => row.Status.ToString() == SelectedStatus).ToArray();
        OnPropertyChanged(nameof(VisibleCount));
    }

    [RelayCommand(CanExecute = nameof(CanRefresh))]
    private async Task RefreshAsync()
    {
        if (!CanRefresh())
            return;

        IsLoading = true;
        RetrievalFailed = false;
        ExportMessage = "";
        var branch = Branch.Trim();
        var scenario = Scenario;
        var source = scenario == "Live" ? branch : $"{branch} · test mode {scenario}";
        RetrievalMessage = $"Retrieving shipments for {source}…";
        if (_snapshot is not null)
            SnapshotLabel = $"Previously loaded · {_snapshot}";

        try
        {
            _shipments = await client.FetchAsync(Address, branch, scenario, TimeoutSeconds);
            _snapshot = $"{source} · {DateTime.Now:yyyy-MM-dd HH:mm:ss} local";
            SnapshotLabel = $"Loaded · {_snapshot}";
            OnPropertyChanged(nameof(Total));
            OnPropertyChanged(nameof(Pending));
            OnPropertyChanged(nameof(InTransit));
            OnPropertyChanged(nameof(Delivered));
            OnPropertyChanged(nameof(Exceptions));
            ApplyFilter();
            RetrievalMessage = Total == 0
                ? $"No shipments found for {source}."
                : $"Loaded {Total} shipment(s) for {source}.";
        }
        catch (ShipmentRetrievalException ex)
        {
            RetrievalFailed = true;
            RetrievalMessage = $"Refresh failed. {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private bool CanRefresh() => CanEditSettings;
    private bool CanExport() => CanEditSettings;

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportAsync()
    {
        if (!CanExport())
            return;

        var rows = VisibleShipments;
        if (rows.Length == 0)
        {
            ExportMessage = "There are no displayed shipments to export.";
            return;
        }

        IsExporting = true;
        ExportMessage = "";
        try
        {
            var fileName = $"HubLink-{rows[0].TrackingNumber}-{DateTime.Now:yyyyMMdd-HHmmss}.csv";
            await using (var stream = await openExport(fileName))
            {
                if (stream is null)
                {
                    ExportMessage = "Export canceled.";
                    return;
                }
                await CsvExport.WriteAsync(stream, rows);
            }
            ExportMessage = $"Exported {rows.Length} shipment(s) to CSV.";
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException
            or ArgumentException or System.Runtime.InteropServices.COMException)
        {
            ExportMessage = $"Export failed. {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }
}
