# ContainerConfigurationMonitor
[![Build Status](https://travis-ci.org/EldieTurner/ContainerConfigurationMonitor.svg?branch=main)](https://travis-ci.org/EldieTurner/ContainerConfigurationMonitor)

A .NET library for monitoring configuration file changes in containers.

## Features

- Monitors configuration file changes using `IContainerFileWatcher`.
- Reloads configuration upon file changes.
- Debounces rapid configuration changes to prevent multiple reloads.
- Integrates with `IOptionsMonitor` to handle configuration changes.
- Includes logging support with `ILogger`.

## Installation

You can install the `ContainerConfigurationMonitorService` via NuGet. 

1. Add the package to your project (this step assumes the package is published to a NuGet repository):

    ```sh
    dotnet add package ContainerConfigurationMonitorService
    ```
  Or add the package with Package Manager
  
    ```sh
    Install-Package ContainerConfigurationMonitorService
    ```

2. Register the `ContainerConfigurationMonitorService` in your `Startup.cs` or `Program.cs`:

    ```csharp
    using ContainerConfigurationMonitor;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.SetBasePath(context.HostingEnvironment.ContentRootPath)
                          .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: false);
                })
                .ConfigureServices((context, services) =>
                {
                    services.Configure<AppSettings>(context.Configuration.GetSection("Settings"));
                    services.AddContainerConfigurationMonitorService("Config/appsettings.json");
                    services.AddHostedService<MyService>();
                });
    }
    ```

## Usage

### Registering the Service

To use the `ContainerConfigurationMonitorService`, register it with the dependency injection container:

```csharp
using ContainerConfigurationMonitor;
using Microsoft.Extensions.DependencyInjection;

public void ConfigureServices(IServiceCollection services)
{
    services.AddContainerConfigurationMonitorService("Config/appsettings.json");
}
```
## Using Debounced OnChange with IOptionsMonitor

You can use the OnChangeDebouncer extension method to debounce the OnChange event of `IOptionsMonitor`:

```csharp
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

public class MyService : IHostedService
{
    private readonly IOptionsMonitor<AppSettings> _optionsMonitor;
    private AppSettings _currentSettings;
    private IDisposable _changeListener;

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
        _changeListener.Dispose();
        return Task.CompletedTask;
    }
}
```

## Sample Configuration

Ensure your `appsettings.jso`n file is correctly set up in the `Config` directory:

```json
{
    "Settings": {
        "Setting1": "Value1",
        "Setting2": "Value2"
    }
}
```

## Logging

The `ContainerConfigurationMonitorService` supports logging via `ILogger`. Ensure you have logging configured in your application to capture logs from the service:

```csharp
using Microsoft.Extensions.Logging;

public static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        })
        .ConfigureServices((context, services) =>
        {
            // Your service registrations
        });

```

MIT License