using StackExchange.Redis;

namespace BeatTogether.Extensions.Redis.Abstractions
{
    public interface IConnectionMultiplexerPool
    {
        IConnectionMultiplexer GetConnection();
    }
}
