using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace BeatTogether.Extensions.Configuration
{
    public static class HostBuilderConfiguration
    {
        public static IHostBuilder ConfigureAppConfiguration(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureAppConfiguration((hostBuilderContext, configurationBuilder) =>
            {
                configurationBuilder
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", true)
                    .AddJsonFile($"appsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", true)
                    .AddJsonFile("sharedsettings.json", true)
                    .AddJsonFile($"sharedsettings.{hostBuilderContext.HostingEnvironment.EnvironmentName}.json", true)
                    .AddEnvironmentVariables();
            });
    }
}
