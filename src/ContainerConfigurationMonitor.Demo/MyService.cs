using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace ContainerConfigurationMonitor.Demo;

public class MyService : IHostedService
{
    private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
    private AppSettings _currentSettings;
    private readonly TimeSpan _debounceTime = TimeSpan.FromMilliseconds(100);
    private Timer _debounceTimer;
    private readonly IDisposable _changeListener;

    public MyService(IOptionsMonitor<AppSettings> optionsMonitor)
    {
        _optionsMonitor = optionsMonitor;
        _currentSettings = _optionsMonitor.CurrentValue;

        // Register for changes with debounce
        _changeListener = _optionsMonitor.OnChangeDebouncer(OnSettingsChanged, TimeSpan.FromMilliseconds(100));
    }

    private void OnSettingsChanged(AppSettings newSettings)
    {
        _currentSettings = newSettings;
        Console.WriteLine($"Settings changed: Setting1={_currentSettings.Setting1}, Setting2={_currentSettings.Setting2}");

    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("MyService started.");
        Console.WriteLine($"Initial settings: Setting1={_currentSettings.Setting1}, Setting2={_currentSettings.Setting2}");
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("MyService stopped.");
        _debounceTimer?.Dispose();
        _changeListener.Dispose();
        return Task.CompletedTask;
    }
}
