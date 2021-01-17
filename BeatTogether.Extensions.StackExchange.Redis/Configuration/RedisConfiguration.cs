namespace BeatTogether.Extensions.StackExchange.Redis.Configuration
{
    public class RedisConfiguration
    {
        public class ConnectionPoolConfiguration
        {
            public int Size { get; set; } = 1;
        }

        public string EndPoint { get; set; } = "localhost:6379";
        public ConnectionPoolConfiguration ConnectionPool { get; set; } = new();
    }
}
