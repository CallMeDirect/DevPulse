using DevPulse.Models;
using DevPulse.Services;

namespace DevPulse.Tests;

/// <summary>
/// Provides deterministic system statistics or failures for view-model tests.
/// </summary>
internal sealed class FakeSystemMonitorService : ISystemMonitorService
{
    #region Private fields

    /// <summary>Defines the behavior executed for each statistics request.</summary>
    private readonly Func<CancellationToken, Task<SystemStats>> _getStats;

    #endregion

    #region Public constructors

    /// <summary>Initializes a fake service that always returns the supplied statistics.</summary>
    /// <param name="stats">The deterministic snapshot returned by the fake.</param>
    public FakeSystemMonitorService(SystemStats stats)
    {
        _getStats = cancellationToken =>
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(stats);
        };
    }

    /// <summary>Initializes a fake service that always fails with the supplied exception.</summary>
    /// <param name="exception">The deterministic exception returned by the fake.</param>
    public FakeSystemMonitorService(Exception exception)
    {
        _getStats = _ =>
            Task.FromException<SystemStats>(exception);
    }

    #endregion

    #region Public methods

    /// <inheritdoc />
    public Task<SystemStats> GetStatsAsync(
        CancellationToken cancellationToken)
    {
        return _getStats(cancellationToken);
    }

    #endregion
}
