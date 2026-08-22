using System;
using System.Threading;
using Avalonia.Controls;
using DevPulse.ViewModels;

namespace DevPulse.Views;

public partial class MainWindow : Window
{
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public MainWindow()
    {
        InitializeComponent();
    }
    
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

    protected override void OnClosed(EventArgs e)
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource.Dispose();

        base.OnClosed(e);
    }
}