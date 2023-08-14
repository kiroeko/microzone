using MicrozoneDaemon;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Logging.EventLog;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "Microzone Daemon";
    })
    .ConfigureServices((context, services) =>
    {
        LoggerProviderOptions.RegisterProviderOptions<
            EventLogSettings, EventLogLoggerProvider>(services);

        services.AddMicrozoneDaemonService();

        services.AddLogging(builder =>
        {
            builder.AddConfiguration(
                context.Configuration.GetSection("Logging"));
        });
    })
    .Build();

await host.RunAsync();