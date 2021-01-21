using BeatTogether.Extensions.StackExchange.Redis.Abstractions;
using BeatTogether.Extensions.StackExchange.Redis.Configuration;
using BeatTogether.Extensions.StackExchange.Redis.Implementations;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace BeatTogether.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddStackExchangeRedis(this IServiceCollection services) =>
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
                );
    }
}
