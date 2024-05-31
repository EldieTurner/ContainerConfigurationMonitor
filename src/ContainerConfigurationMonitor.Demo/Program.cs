using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ContainerConfigurationMonitor.Demo
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var configDestinationPath = Path.Combine("/config", "appsettings.json");

            return Host.CreateDefaultBuilder(args)
               .UseSystemd()
               .ConfigureAppConfiguration((hostingContext, config) =>
               {
                   var env = hostingContext.HostingEnvironment;
                   var basePath = Directory.GetCurrentDirectory();
                   if (!File.Exists(configDestinationPath))
                       File.Copy(Path.Combine(basePath, "Config", "appsettings.json"), configDestinationPath, true);

                   config.AddJsonFile(configDestinationPath, optional: true, reloadOnChange: true);
                   //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                   //config.AddEnvironmentVariables();
               })
               .ConfigureLogging((hostingContext, logging) =>
               {
                   logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                   logging.AddSimpleConsole(options =>
                   {
                       options.TimestampFormat = "MM/dd/yyyy hh:mm:ss ";
                   });
                   logging.AddDebug();
               })
               .ConfigureServices((hostContext, services) =>
               {
                   services.Configure<AppSettings>(hostContext.Configuration.GetSection("Settings"));
                   services.AddContainerConfigurationMonitorService(configDestinationPath);
                   services.AddHostedService<MyService>();
               });
        }
    }
}
