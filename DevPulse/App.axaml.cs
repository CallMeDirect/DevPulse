using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DevPulse.ViewModels;
using DevPulse.Views;
using DevPulse.Services;

namespace DevPulse;

/// <summary>
/// Configures the Avalonia application and composes the main dashboard dependencies.
/// </summary>
public partial class App : Application
{
    #region Public methods

    /// <summary>Loads application-level Avalonia XAML resources.</summary>
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <summary>Creates the desktop window, monitoring service, and main view model.</summary>
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            SystemMonitorService systemMonitorService = new();
            MainViewModel mainViewModel = new(systemMonitorService);
            
            desktop.MainWindow = new MainWindow
            {
                DataContext = mainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    #endregion
}
