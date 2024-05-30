using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;
using ContainerFileSystemWatcher;
using System.Linq;

namespace ContainerConfigurationMonitor
{
    public class ContainerConfigurationMonitorService : IContainerConfigurationMonitorService
    {
        private readonly IContainerFileWatcher _fileWatcher;
        private readonly IConfigurationRoot _configurationRoot;
        private readonly string _configFilePath;
        private readonly TimeSpan _pollingInterval = TimeSpan.FromSeconds(5);
        public ContainerConfigurationMonitorService(IContainerFileWatcher fileWatcher, IConfiguration configuration)
        {
            _fileWatcher = fileWatcher;
            _configurationRoot = (IConfigurationRoot)configuration;
            _configFilePath = _configurationRoot.Providers
                .OfType<Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider>()
                .First()
                .Source.Path;

            _fileWatcher.OnFileChanged += OnConfigurationFileChanged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.AddWatch(_configFilePath, _pollingInterval);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _fileWatcher.RemoveWatch(_configFilePath);
            return Task.CompletedTask;
        }

        private void OnConfigurationFileChanged(ChangeType changeType, string filePath)
        {
            if (changeType == ChangeType.Modified && filePath == _configFilePath)
            {
                _configurationRoot.Reload();
            }
        }
    }
}
