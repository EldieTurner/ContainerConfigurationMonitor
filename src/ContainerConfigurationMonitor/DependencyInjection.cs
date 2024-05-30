using ContainerFileSystemWatcher;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace ContainerConfigurationMonitor
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddContainerConfigurationMonitorService(this IServiceCollection services)
        {
            services.AddContainerFileSystemWatcher();
            services.AddHostedService<ContainerConfigurationMonitorService>();
            return services;
        }
    }
}
