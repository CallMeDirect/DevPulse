using System.Threading;
using System.Threading.Tasks;
using DevPulse.Models;

namespace DevPulse.Services;

public interface ISystemMonitorService
{
    Task<SystemStats> GetStatsAsync(CancellationToken cancellationToken);
}