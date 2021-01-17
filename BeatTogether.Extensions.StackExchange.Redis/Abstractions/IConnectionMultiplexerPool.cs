using StackExchange.Redis;

namespace BeatTogether.Extensions.StackExchange.Redis.Abstractions
{
    public interface IConnectionMultiplexerPool
    {
        IConnectionMultiplexer GetConnection();
    }
}
