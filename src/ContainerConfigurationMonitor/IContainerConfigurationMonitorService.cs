using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerConfigurationMonitor
{
    public interface IContainerConfigurationMonitorService : IHostedService
    {
        Task StartAsync(CancellationToken cancellationToken);
        Task StopAsync(CancellationToken cancellationToken);
    }
}