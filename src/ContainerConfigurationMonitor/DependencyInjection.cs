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

        public static IDisposable OnChangeDebouncer<TOptions>(this IOptionsMonitor<TOptions> optionsMonitor, Action<TOptions> listener, TimeSpan debounceTime)
        {
            Timer debounceTimer = null;
            TOptions latestOptions = default(TOptions);
            object lockObj = new object();

            void OnChange(TOptions options)
            {
                lock (lockObj)
                {
                    latestOptions = options;
                    if (debounceTimer == null)
                    {
                        debounceTimer = new Timer(state =>
                        {
                            lock (lockObj)
                            {
                                listener(latestOptions);
                                debounceTimer.Dispose();
                                debounceTimer = null;
                            }
                        }, null, debounceTime, Timeout.InfiniteTimeSpan);
                    }
                    else
                    {
                        debounceTimer.Change(debounceTime, Timeout.InfiniteTimeSpan);
                    }
                }
            }
            return optionsMonitor.OnChange(OnChange);
        }
    }
}
