using Microsoft.Extensions.Options;
using System.Threading;
using System;

namespace ContainerConfigurationMonitor
{
    public static  class ExtensionMethods
    {

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
