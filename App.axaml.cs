using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using HubLink.Services;
using HubLink.ViewModels;
using HubLink.Views;

namespace HubLink;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
            var window = new MainWindow();
            window.DataContext = new MainViewModel(new ShipmentClient(http), window.OpenExportAsync);
            desktop.MainWindow = window;
            desktop.Exit += (_, _) => http.Dispose();
        }

        base.OnFrameworkInitializationCompleted();
    }
}
