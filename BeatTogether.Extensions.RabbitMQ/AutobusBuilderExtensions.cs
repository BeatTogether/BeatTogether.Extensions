using Autobus;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace BeatTogether.Extensions
{
    public static class AutobusBuilderExtensions
    {
        public static IAutobusBuilder UseRabbitMQTransport(
            this IAutobusBuilder autobusBuilder,
            IConfiguration configuration) =>
            autobusBuilder.UseRabbitMQTransport(builder =>
            {
                configuration = configuration.GetSection("RabbitMQ");
                builder.ConfigureConnectionFactory(connectionFactory =>
                {
                    connectionFactory.HostName = configuration.GetValue("HostName", "localhost");
                    connectionFactory.Port = configuration.GetValue("Port", 5672);
                    connectionFactory.VirtualHost = configuration.GetValue("VirtualHost", ConnectionFactory.DefaultVHost);
                    connectionFactory.UserName = configuration.GetValue("UserName", ConnectionFactory.DefaultUser);
                    connectionFactory.Password = configuration.GetValue("Password", ConnectionFactory.DefaultPass);
                });

                var prefetchSize = configuration.GetValue("Qos:PrefetchSize", 0U);
                var prefetchCount = configuration.GetValue<ushort>("Qos:PrefetchCount", 0);
                builder.ConfigureQos(prefetchSize, prefetchCount);

                if (configuration.GetValue("UseConsistentHashing", false))
                    builder.UseConsistentHashing();
            });
    }
}
