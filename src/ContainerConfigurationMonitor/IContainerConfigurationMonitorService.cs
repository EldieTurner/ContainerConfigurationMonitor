using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace ContainerConfigurationMonitor
{
    /// <summary>
    /// Defines the interface for a container configuration monitor service.
    /// </summary>
    public interface IContainerConfigurationMonitorService : IHostedService
    {
        /// <summary>
        /// Starts the monitoring service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        Task StartAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Stops the monitoring service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        Task StopAsync(CancellationToken cancellationToken);
    }
}