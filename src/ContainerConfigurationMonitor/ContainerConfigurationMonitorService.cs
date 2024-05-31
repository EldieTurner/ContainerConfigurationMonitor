using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using ContainerFileSystemWatcher;
using Microsoft.Extensions.Logging;

namespace ContainerConfigurationMonitor
{
    public class ContainerConfigurationMonitorService : IContainerConfigurationMonitorService
    {
        private readonly IContainerFileWatcher _fileWatcher;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly string _configFilePath;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        private readonly string _directoryPath;
        private readonly ILogger<ContainerConfigurationMonitorService> _logger;

        public ContainerConfigurationMonitorService(ILogger<ContainerConfigurationMonitorService> logger, IContainerFileWatcher fileWatcher, IConfiguration configuration, string configFilePath)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fileWatcher = fileWatcher ?? throw new ArgumentNullException(nameof(fileWatcher));
            _configurationRoot = configuration as IConfigurationRoot ?? throw new ArgumentException("Configuration must be an IConfigurationRoot", nameof(configuration));
            _configFilePath = configFilePath ?? throw new ArgumentNullException(nameof(configFilePath));
            _directoryPath = System.IO.Path.GetDirectoryName(_configFilePath);
            _fileWatcher.OnFileChanged += OnConfigurationFileChanged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.AddWatch(_directoryPath, _pollingInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.RemoveWatch(_directoryPath);
            return Task.CompletedTask;
        }

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
