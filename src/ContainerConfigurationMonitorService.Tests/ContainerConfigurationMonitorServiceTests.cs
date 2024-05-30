using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using Microsoft.Extensions.Configuration;
using ContainerFileSystemWatcher;
using ContainerConfigurationMonitor;

[TestClass]
public class ContainerConfigurationMonitorServiceTests
{
    private Mock<IContainerFileWatcher> _mockFileWatcher;
    private Mock<IConfigurationRoot> _mockConfigurationRoot;
    private ContainerConfigurationMonitorService _service;
    private string _configFilePath = "appsettings.json";

    [TestInitialize]
    public void SetUp()
    {
        _mockFileWatcher = new Mock<IContainerFileWatcher>();
        _mockConfigurationRoot = new Mock<IConfigurationRoot>();

        var configurationProvider = new Microsoft.Extensions.Configuration.Json.JsonConfigurationProvider(new Microsoft.Extensions.Configuration.Json.JsonConfigurationSource { Path = _configFilePath });
        _mockConfigurationRoot.Setup(c => c.Providers).Returns(new List<IConfigurationProvider> { configurationProvider });

        _service = new ContainerConfigurationMonitorService(_mockFileWatcher.Object, _mockConfigurationRoot.Object);
    }

    [TestMethod]
    public async Task StartAsync_ShouldAddWatch()
    {
        await _service.StartAsync(CancellationToken.None);

        _mockFileWatcher.Verify(fw => fw.AddWatch(It.Is<string>(s => s == _configFilePath), It.IsAny<TimeSpan>()), Times.Once);
    }

    [TestMethod]
    public async Task StopAsync_ShouldRemoveWatch()
    {
        await _service.StopAsync(CancellationToken.None);

        _mockFileWatcher.Verify(fw => fw.RemoveWatch(It.Is<string>(s => s == _configFilePath)), Times.Once);
    }

    [TestMethod]
    public void OnConfigurationFileChanged_ShouldReloadConfiguration_WhenFileChanged()
    {
        _mockFileWatcher.Raise(fw => fw.OnFileChanged += null, ChangeType.Modified, _configFilePath);

        _mockConfigurationRoot.Verify(cr => cr.Reload(), Times.Once);
    }

    [TestMethod]
    public void OnConfigurationFileChanged_ShouldNotReloadConfiguration_WhenDifferentFileChanged()
    {
        _mockFileWatcher.Raise(fw => fw.OnFileChanged += null, ChangeType.Modified, "differentfile.json");

        _mockConfigurationRoot.Verify(cr => cr.Reload(), Times.Never);
    }
}
