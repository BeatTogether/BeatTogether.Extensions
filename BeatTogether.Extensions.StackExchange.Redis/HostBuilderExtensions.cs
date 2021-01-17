using BeatTogether.Extensions.StackExchange.Redis.Abstractions;
using BeatTogether.Extensions.StackExchange.Redis.Configuration;
using BeatTogether.Extensions.StackExchange.Redis.Implementations;
using Depths.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace Depths.Extensions.StackExchange.Redis
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseRedis(this IHostBuilder hostBuilder) =>
            hostBuilder.ConfigureServices(services =>
                services
                    .AddConfiguration<RedisConfiguration>("Redis")
                    .AddSingleton(serviceProvider =>
                    {
                        var configuration = serviceProvider.GetRequiredService<RedisConfiguration>();
                        var connectionMultiplexerConfiguration = new ConfigurationOptions
                        {
                            AbortOnConnectFail = false
                        };
                        connectionMultiplexerConfiguration.EndPoints.Add(configuration.EndPoint);
                        return connectionMultiplexerConfiguration;
                    })
                    .AddSingleton<IConnectionMultiplexerPool, ConnectionMultiplexerPool>()
                    .AddScoped(serviceProvider =>
                        serviceProvider
                            .GetRequiredService<IConnectionMultiplexerPool>()
                            .GetConnection()
                    )
            );
    }
}
