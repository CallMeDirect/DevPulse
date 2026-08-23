using System;
using System.Threading;
using Avalonia.Controls;
using DevPulse.ViewModels;

namespace DevPulse.Views;

/// <summary>
/// Hosts the DevPulse dashboard and coordinates monitoring with the window lifetime.
/// </summary>
public partial class MainWindow : Window
{
    #region Private fields

    /// <summary>Signals the monitoring loop to stop when the window closes.</summary>
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    #endregion

    #region Public constructors

    /// <summary>Initializes a new instance of the main dashboard window.</summary>
    public MainWindow()
    {
        InitializeComponent();
    }
    
    #endregion

    #region Protected methods

    /// <summary>Starts resource monitoring after the window has opened.</summary>
    /// <param name="e">The window-opened event arguments.</param>
    protected override async void OnOpened(EventArgs e)
    {
        base.OnOpened(e);

        if (DataContext is not MainViewModel viewModel)
            return;

        try
        {
            await viewModel.StartMonitoringAsync(
                _cancellationTokenSource.Token);
        }
        catch (OperationCanceledException)
        {
            // The window closed - the cancellation is obvious.
        }
    }

    /// <summary>Cancels monitoring and releases lifetime resources when the window closes.</summary>
    /// <param name="e">The window-closed event arguments.</param>
    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        base.OnClosed(e);
    }

    #endregion
}
