using ContainerFileSystemWatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Threading;
using System;
using Microsoft.Extensions.Logging;

namespace ContainerConfigurationMonitor
{
    /// <summary>
    /// Provides extension methods for service registration.
    /// </summary>
    public static class DependencyInjection
    {
        /// <summary>
        /// Adds the ContainerConfigurationMonitorService to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configFilePath">The path to the configuration file.</param>
        /// <returns>The updated service collection.</returns>
        public static IServiceCollection AddContainerConfigurationMonitorService(this IServiceCollection services, string configFilePath)
        {
            services.AddContainerFileSystemWatcher();
            services.AddSingleton<IHostedService>(provider =>
                                    new ContainerConfigurationMonitorService(
                                        provider.GetRequiredService<ILogger<ContainerConfigurationMonitorService>>(),
                                        provider.GetRequiredService<IContainerFileWatcher>(),
                                        provider.GetRequiredService<IConfiguration>(),
                                        configFilePath));
            return services;
        }
    }
}