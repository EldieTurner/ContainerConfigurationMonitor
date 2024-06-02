using Microsoft.Extensions.Options;
using System.Threading;
using System;

namespace ContainerConfigurationMonitor
{
    /// <summary>
    /// Provides extension methods for handling options changes with debounce functionality.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Registers a listener to be called whenever the options change, with a debounce time.
        /// </summary>
        /// <typeparam name="TOptions">The type of options being monitored.</typeparam>
        /// <param name="optionsMonitor">The options monitor.</param>
        /// <param name="listener">The listener to call on options change.</param>
        /// <param name="debounceTime">The debounce time.</param>
        /// <returns>An IDisposable that can be used to unregister the listener.</returns>
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