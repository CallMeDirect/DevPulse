using System.Threading;
using System.Threading.Tasks;
using DevPulse.Models;

namespace DevPulse.Services;

/// <summary>
/// Defines a source of current CPU, memory, and system-drive statistics.
/// </summary>
public interface ISystemMonitorService
{
    #region Public methods

    /// <summary>
    /// Retrieves a snapshot of the current system resource usage.
    /// </summary>
    /// <param name="cancellationToken">A token used to cancel the pending operation.</param>
    /// <returns>A task containing the latest system statistics.</returns>
    Task<SystemStats> GetStatsAsync(CancellationToken cancellationToken);

    #endregion
}
