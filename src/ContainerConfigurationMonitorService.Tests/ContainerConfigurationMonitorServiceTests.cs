using Moq;
using Microsoft.Extensions.Configuration;
using ContainerFileSystemWatcher;
using Microsoft.Extensions.Logging;

namespace ContainerConfigurationMonitor.Tests;

[TestClass]
public class ContainerConfigurationMonitorServiceTests
{
    private TestContainerFileWatcher _testFileWatcher;
    private Mock<IConfigurationRoot> _mockConfigurationRoot;
    private ContainerConfigurationMonitorService _service;
    private string _configFilePath = "/config/appsettings.json";
    private string _directoryPath;

    [TestInitialize]
    public void SetUp()
    {
        var _logger = new Mock<ILogger<ContainerConfigurationMonitorService>>();
        _testFileWatcher = new TestContainerFileWatcher();
        _mockConfigurationRoot = new Mock<IConfigurationRoot>();

        var configurationProvider = new Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider(new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource { Path = _configFilePath });
        _mockConfigurationRoot.Setup(c => c.Providers).Returns(new List<IConfigurationProvider> { configurationProvider });

        _directoryPath = System.IO.Path.GetDirectoryName(_configFilePath);
        _service = new ContainerConfigurationMonitorService(_logger.Object, _testFileWatcher, _mockConfigurationRoot.Object, _configFilePath);

        Console.WriteLine($"Test Setup: _configFilePath = {_configFilePath}, _directoryPath = {_directoryPath}");
    }

    [TestMethod]
    public async Task StartAsync_ShouldAddWatch()
    {
        await _service.StartAsync(CancellationToken.None);

        Assert.IsTrue(_testFileWatcher.IsWatching(_directoryPath));
    }

    [TestMethod]
    public async Task StopAsync_ShouldRemoveWatch()
    {
        await _service.StartAsync(CancellationToken.None);
        await _service.StopAsync(CancellationToken.None);

        Assert.IsFalse(_testFileWatcher.IsWatching(_directoryPath));
    }

    [TestMethod]
    public void OnConfigurationFileChanged_ShouldReloadConfiguration_WhenFileChanged()
    {
        _service.StartAsync(CancellationToken.None).Wait();

        _testFileWatcher.SimulateFileChange(ChangeType.Modified, _configFilePath);

        _mockConfigurationRoot.Verify(cr => cr.Reload(), Times.Once);
    }

    [TestMethod]
    public void OnConfigurationFileChanged_ShouldNotReloadConfiguration_WhenDifferentFileChanged()
    {
        _service.StartAsync(CancellationToken.None).Wait();

        _testFileWatcher.SimulateFileChange(ChangeType.Modified, "differentfile.json");

        _mockConfigurationRoot.Verify(cr => cr.Reload(), Times.Never);
    }
}