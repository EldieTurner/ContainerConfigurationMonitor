using ContainerFileSystemWatcher;

namespace ContainerConfigurationMonitor.Tests;

public class TestContainerFileWatcher : IContainerFileWatcher
{
    private readonly Dictionary<string, bool> _watchers = new Dictionary<string, bool>();
    public bool EnableLogging { get; set; }

    public event Action<ChangeType, string> OnFileChanged;

    public void AddWatch(string path, TimeSpan pollingInterval)
    {
        if (!_watchers.ContainsKey(path))
        {
            _watchers[path] = true;
        }
    }

    public void RemoveWatch(string path)
    {
        if (_watchers.ContainsKey(path))
        {
            _watchers.Remove(path);
        }
    }

    public bool IsWatching(string path)
    {
        return _watchers.ContainsKey(path);
    }

    public void SimulateFileChange(ChangeType changeType, string path)
    {
        OnFileChanged?.Invoke(changeType, path);
    }
}