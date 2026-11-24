using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using HubLink.Models;

namespace HubLink.Services;

public sealed class ShipmentClient(HttpClient http)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<Shipment[]> FetchAsync(string address, string branch, string scenario, int timeoutSeconds)
    {
        if (!Uri.TryCreate(address.Trim(), UriKind.Absolute, out var baseUri) ||
            baseUri.Scheme is not ("http" or "https") || baseUri.Query.Length > 0 ||
            baseUri.Fragment.Length > 0 || baseUri.UserInfo.Length > 0)
            throw new ShipmentRetrievalException("Enter an HTTP or HTTPS service address without credentials or a query.");

        branch = branch.Trim();
        if (!Regex.IsMatch(branch, @"\ABR-[0-9]{3}\z"))
            throw new ShipmentRetrievalException("Enter a branch code such as BR-001 (uppercase).");

        var path = scenario == "Live" ? "/api/shipments" : "/api/test/shipments";
        var query = $"branchCode={Uri.EscapeDataString(branch)}";
        if (scenario != "Live")
            query += $"&mode={Uri.EscapeDataString(scenario)}";
        var url = new Uri($"{baseUri.AbsoluteUri.TrimEnd('/')}{path}?{query}");

        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(timeoutSeconds));
        try
        {
            using var response = await http.GetAsync(url, timeout.Token).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
                throw new ShipmentRetrievalException($"The service returned HTTP {(int)response.StatusCode} ({response.ReasonPhrase}).");

            var json = await response.Content.ReadAsStringAsync(timeout.Token).ConfigureAwait(false);
            var payload = JsonSerializer.Deserialize<ShipmentEnvelope>(json, JsonOptions);
            if (payload?.BranchCode != branch || payload.Shipments is null)
                throw new JsonException("The envelope does not match the requested branch.");

            var shipments = new List<Shipment>(payload.Shipments.Length);
            var trackingNumbers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var item in payload.Shipments)
            {
                if (item is null || item.BranchCode != branch ||
                    string.IsNullOrWhiteSpace(item.TrackingNumber) || !trackingNumbers.Add(item.TrackingNumber) ||
                    string.IsNullOrWhiteSpace(item.Origin) || string.IsNullOrWhiteSpace(item.Destination) ||
                    !Enum.TryParse<ShipmentStatus>(item.Status, out var status) || status.ToString() != item.Status ||
                    !DateTimeOffset.TryParse(item.LastUpdatedUtc, CultureInfo.InvariantCulture,
                        DateTimeStyles.None, out var updated) || updated.Offset != TimeSpan.Zero ||
                    !(item.LastUpdatedUtc!.EndsWith('Z') || item.LastUpdatedUtc.EndsWith("+00:00", StringComparison.Ordinal)))
                    throw new JsonException("A shipment record is missing or invalid.");

                shipments.Add(new Shipment(item.TrackingNumber, item.Origin, item.Destination, status, updated));
            }
            return shipments.ToArray();
        }
        catch (OperationCanceledException) when (timeout.IsCancellationRequested)
        {
            throw new ShipmentRetrievalException($"The request timed out after {timeoutSeconds} seconds. Try refreshing again.");
        }
        catch (HttpRequestException ex)
        {
            throw new ShipmentRetrievalException("Could not reach the shipment service. Check the address and whether the server is running.", ex);
        }
        catch (JsonException ex)
        {
            throw new ShipmentRetrievalException("The service returned invalid shipment data.", ex);
        }
    }

    private sealed record ShipmentEnvelope(string? BranchCode, ShipmentDto?[]? Shipments);

    private sealed record ShipmentDto(string? TrackingNumber, string? BranchCode, string? Origin,
        string? Destination, string? Status, string? LastUpdatedUtc)
    {
    }
}

public sealed class ShipmentRetrievalException(string message, Exception? inner = null) : Exception(message, inner);
