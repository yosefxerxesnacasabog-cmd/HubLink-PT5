using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace HubLink.Views;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    public async Task<Stream?> OpenExportAsync(string suggestedName)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Export displayed shipments",
            SuggestedFileName = suggestedName,
            DefaultExtension = "csv",
            ShowOverwritePrompt = true,
            FileTypeChoices = [new FilePickerFileType("CSV file") { Patterns = ["*.csv"] }]
        });
        return file is null ? null : await file.OpenWriteAsync();
    }
}
