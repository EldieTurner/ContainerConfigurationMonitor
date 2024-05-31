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
    public static class DependencyInjection
    {
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
