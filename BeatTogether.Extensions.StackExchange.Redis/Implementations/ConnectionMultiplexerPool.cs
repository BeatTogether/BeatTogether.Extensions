using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using BeatTogether.Extensions.StackExchange.Redis.Abstractions;
using StackExchange.Redis;
using StackExchange.Redis.Profiling;

namespace BeatTogether.Extensions.StackExchange.Redis.Implementations
{
    public class ConnectionMultiplexerPool : IConnectionMultiplexerPool, IDisposable
    {
        private class PooledConnectionMultiplexer : IConnectionMultiplexer
        {
            private readonly IConnectionMultiplexer _connectionMultiplexer;

            public string ClientName => _connectionMultiplexer.ClientName;
            public string Configuration => _connectionMultiplexer.Configuration;
            public int TimeoutMilliseconds => _connectionMultiplexer.TimeoutMilliseconds;
            public long OperationCount => _connectionMultiplexer.OperationCount;

            [Obsolete]
            public bool PreserveAsyncOrder
            {
                get => _connectionMultiplexer.PreserveAsyncOrder;
                set => _connectionMultiplexer.PreserveAsyncOrder = value;
            }

            public bool IsConnected => _connectionMultiplexer.IsConnected;
            public bool IsConnecting => _connectionMultiplexer.IsConnecting;

            public bool IncludeDetailInExceptions
            {
                get => _connectionMultiplexer.IncludeDetailInExceptions;
                set => _connectionMultiplexer.IncludeDetailInExceptions = value;
            }
            public int StormLogThreshold
            {
                get => _connectionMultiplexer.StormLogThreshold;
                set => _connectionMultiplexer.StormLogThreshold = value;
            }

            public event EventHandler<RedisErrorEventArgs> ErrorMessage
            {
                add => _connectionMultiplexer.ErrorMessage += value;
                remove => _connectionMultiplexer.ErrorMessage -= value;
            }

            public event EventHandler<ConnectionFailedEventArgs> ConnectionFailed
            {
                add => _connectionMultiplexer.ConnectionFailed += value;
                remove => _connectionMultiplexer.ConnectionFailed -= value;
            }

            public event EventHandler<InternalErrorEventArgs> InternalError
            {
                add => _connectionMultiplexer.InternalError += value;
                remove => _connectionMultiplexer.InternalError -= value;
            }

            public event EventHandler<ConnectionFailedEventArgs> ConnectionRestored
            {
                add => _connectionMultiplexer.ConnectionRestored += value;
                remove => _connectionMultiplexer.ConnectionRestored -= value;
            }

            public event EventHandler<EndPointEventArgs> ConfigurationChanged
            {
                add => _connectionMultiplexer.ConfigurationChanged += value;
                remove => _connectionMultiplexer.ConfigurationChanged -= value;
            }

            public event EventHandler<EndPointEventArgs> ConfigurationChangedBroadcast
            {
                add => _connectionMultiplexer.ConfigurationChangedBroadcast += value;
                remove => _connectionMultiplexer.ConfigurationChangedBroadcast -= value;
            }

            public event EventHandler<HashSlotMovedEventArgs> HashSlotMoved
            {
                add => _connectionMultiplexer.HashSlotMoved += value;
                remove => _connectionMultiplexer.HashSlotMoved -= value;
            }

            public PooledConnectionMultiplexer(IConnectionMultiplexer connectionMultiplexer)
            {
                _connectionMultiplexer = connectionMultiplexer;
            }

            public void Close(bool allowCommandsToComplete = true)
                => _connectionMultiplexer.Close(allowCommandsToComplete);

            public Task CloseAsync(bool allowCommandsToComplete = true)
                => _connectionMultiplexer.CloseAsync(allowCommandsToComplete);

            public bool Configure(TextWriter? log = null)
                => _connectionMultiplexer.Configure(log);

            public Task<bool> ConfigureAsync(TextWriter? log = null)
                => _connectionMultiplexer.ConfigureAsync(log);

            public void Dispose()
            {
            }

            public void ExportConfiguration(Stream destination, ExportOptions options = (ExportOptions)(-1))
                => _connectionMultiplexer.ExportConfiguration(destination, options);

            public ServerCounters GetCounters()
                => _connectionMultiplexer.GetCounters();

            public IDatabase GetDatabase(int db = -1, object? asyncState = null)
                => _connectionMultiplexer.GetDatabase(db, asyncState);

            public EndPoint[] GetEndPoints(bool configuredOnly = false)
                => _connectionMultiplexer.GetEndPoints(configuredOnly);

            public int GetHashSlot(RedisKey key)
                => _connectionMultiplexer.GetHashSlot(key);

            public IServer GetServer(string host, int port, object? asyncState = null)
                => _connectionMultiplexer.GetServer(host, port, asyncState);

            public IServer GetServer(string hostAndPort, object? asyncState = null)
                => _connectionMultiplexer.GetServer(hostAndPort, asyncState);

            public IServer GetServer(IPAddress host, int port)
                => _connectionMultiplexer.GetServer(host, port);

            public IServer GetServer(EndPoint endpoint, object? asyncState = null)
                => _connectionMultiplexer.GetServer(endpoint, asyncState);

            public string GetStatus()
                => _connectionMultiplexer.GetStatus();

            public void GetStatus(TextWriter log)
                => _connectionMultiplexer.GetStatus(log);

            public string GetStormLog()
                => _connectionMultiplexer.GetStormLog();

            public ISubscriber GetSubscriber(object? asyncState = null)
                => _connectionMultiplexer.GetSubscriber(asyncState);

            public int HashSlot(RedisKey key)
                => _connectionMultiplexer.HashSlot(key);

            public long PublishReconfigure(CommandFlags flags = CommandFlags.None)
                => _connectionMultiplexer.PublishReconfigure(flags);

            public Task<long> PublishReconfigureAsync(CommandFlags flags = CommandFlags.None)
                => _connectionMultiplexer.PublishReconfigureAsync(flags);

            public void RegisterProfiler(Func<ProfilingSession> profilingSessionProvider)
                => _connectionMultiplexer.RegisterProfiler(profilingSessionProvider);

            public void ResetStormLog()
                => _connectionMultiplexer.ResetStormLog();

            public void Wait(Task task)
                => _connectionMultiplexer.Wait(task);

            public T Wait<T>(Task<T> task)
                => _connectionMultiplexer.Wait(task);

            public void WaitAll(params Task[] tasks)
                => _connectionMultiplexer.WaitAll(tasks);

            public static async Task<PooledConnectionMultiplexer> ConnectAsync(ConfigurationOptions options, TextWriter? log = null)
                => new PooledConnectionMultiplexer(await ConnectionMultiplexer.ConnectAsync(options, log));
        }

        private readonly RedisConfiguration _configuration;

        private readonly List<Task<PooledConnectionMultiplexer>> _connections;

        private int _connectionCounter = -1;

        public ConnectionMultiplexerPool(
            RedisConfiguration configuration,
            ConfigurationOptions connectionMultiplexerConfiguration)
        {
            _configuration = configuration;

            if (configuration.ConnectionPool.Size <= 0)
                throw new Exception(
                    "Invalid Redis connection pool size " +
                    $"(Size={configuration.ConnectionPool.Size})."
                );

            _connections = new List<Task<PooledConnectionMultiplexer>>();
            while (_connections.Count < _configuration.ConnectionPool.Size)
                _connections.Add(PooledConnectionMultiplexer.ConnectAsync(connectionMultiplexerConfiguration));
        }

        #region Public Methods

        public IConnectionMultiplexer GetConnection()
        {
            if (_connections.Count > 1)
            {
                var index = (int)(unchecked((uint)Interlocked.Increment(ref _connectionCounter)) % _connections.Count);
                return _connections[index].Result;
            }
            return _connections[0].Result;
        }

        #endregion

        #region IDisposable Methods

        public void Dispose()
        {
            for (var i = 0; i < _configuration.ConnectionPool.Size; i++)
                _connections[i].Dispose();
        }

        #endregion
    }
}
