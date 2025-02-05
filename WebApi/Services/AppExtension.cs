using Serilog;
using Serilog.Formatting.Json;

namespace WebApi.Services;

public static class AppExtension
{
    public static void SerilogConfiguration(this IHostBuilder host)
    {
        host.UseSerilog((context, loggerConfig) =>
        {
            loggerConfig.WriteTo.File(new JsonFormatter(), "logs/applogs.txt", rollingInterval: RollingInterval.Day);
        });
    }
}