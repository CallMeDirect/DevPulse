using Avalonia;
using System;

namespace DevPulse;

/// <summary>
/// Provides the process entry point and Avalonia application configuration.
/// </summary>
internal sealed class Program
{
    #region Public methods

    /// <summary>Starts DevPulse with the classic desktop application lifetime.</summary>
    /// <param name="args">Command-line arguments supplied to the application.</param>
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    /// <summary>Builds the Avalonia application used by the runtime and visual designer.</summary>
    /// <returns>A configured Avalonia application builder.</returns>
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();

    #endregion
}
