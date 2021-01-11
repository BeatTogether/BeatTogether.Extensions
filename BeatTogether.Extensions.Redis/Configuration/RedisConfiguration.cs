namespace BeatTogether.Extensions.Redis.Configuration
{
    public class RedisConfiguration
    {
        public class ConnectionPoolConfiguration
        {
            public int Size { get; set; } = 1;
        }

        public string? Endpoint { get; set; }
        public ConnectionPoolConfiguration ConnectionPool { get; set; } = new();
    }
}
