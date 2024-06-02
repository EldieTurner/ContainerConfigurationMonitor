using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using ContainerFileSystemWatcher;
using Microsoft.Extensions.Logging;

namespace ContainerConfigurationMonitor
{
    /// <summary>
    /// Monitors configuration changes in a specified container and reloads the configuration when changes are detected.
    /// </summary>
    public class ContainerConfigurationMonitorService : IContainerConfigurationMonitorService
    {
        private readonly IContainerFileWatcher _fileWatcher;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly string _configFilePath;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        private readonly string _directoryPath;
        private readonly ILogger<ContainerConfigurationMonitorService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContainerConfigurationMonitorService"/> class.
        /// </summary>
        /// <param name="logger">The logger instance.</param>
        /// <param name="fileWatcher">The file watcher instance.</param>
        /// <param name="configuration">The configuration root.</param>
        /// <param name="configFilePath">The path to the configuration file.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the required parameters are null.</exception>
        /// <exception cref="ArgumentException">Thrown when the configuration is not an IConfigurationRoot.</exception>
        public ContainerConfigurationMonitorService(ILogger<ContainerConfigurationMonitorService> logger, IContainerFileWatcher fileWatcher, IConfiguration configuration, string configFilePath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileWatcher = fileWatcher ?? throw new ArgumentNullException(nameof(fileWatcher));
            _configurationRoot = configuration as IConfigurationRoot ?? throw new ArgumentException("Configuration must be an IConfigurationRoot", nameof(configuration));
            _configFilePath = configFilePath ?? throw new ArgumentNullException(nameof(configFilePath));
            _directoryPath = System.IO.Path.GetDirectoryName(_configFilePath);
            _fileWatcher.OnFileChanged += OnConfigurationFileChanged;
        }

        /// <summary>
        /// Starts the monitoring service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous start operation.</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.AddWatch(_directoryPath, _pollingInterval);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Stops the monitoring service.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A task that represents the asynchronous stop operation.</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.RemoveWatch(_directoryPath);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the configuration file changed event.
        /// </summary>
        /// <param name="changeType">The type of change detected.</param>
        /// <param name="filePath">The path of the changed file.</param>
        private void OnConfigurationFileChanged(ChangeType changeType, string filePath)
        {
            if (changeType == ChangeType.Modified && filePath == _configFilePath)
            {
                _configurationRoot.Reload();
                _logger.LogInformation($"Configuration reloaded at {DateTime.Now}");
            }
        }
    }
}