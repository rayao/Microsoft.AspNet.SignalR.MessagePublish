// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Resources;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Microsoft.AspNet.SignalR.Redis
{
    public class RedisConnection : IRedisConnection
    {
        private readonly ILogger _logger;

        private ConnectionMultiplexer _connection;
        //private ulong _latestMessageId;

        private object _shutdownLock = new object();
        private bool _disposed = false;

        public RedisConnection(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<RedisConnection>();
        }

        public async Task ConnectAsync(string connectionString, TraceSource _)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(RedisConnection));
            }

            var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);

            lock (_shutdownLock)
            {
                if (_disposed)
                {
                    // Nothing to do here, just clean up the connection we created, since we've been closed mid-connection
                    connection.Dispose();
                    return;
                }
                else
                {
                    // We weren't disposed during the connection, so initialize it.
                    _connection = connection;
                    if (!_connection.IsConnected)
                    {
                        _connection.Dispose();
                        _connection = null;
                        throw new InvalidOperationException("Failed to connect to Redis");
                    }

                    _connection.ConnectionFailed += OnConnectionFailed;
                    _connection.ConnectionRestored += OnConnectionRestored;
                    _connection.ErrorMessage += OnError;
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void Close(string key, bool allowCommandsToComplete = true)
        {
            lock (_shutdownLock)
            {
                if (_disposed)
                {
                    return;
                }

                if (_connection != null)
                {
                    _connection.Close(allowCommandsToComplete);
                }

                _connection.Dispose();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            lock (_shutdownLock)
            {
                if (_disposed)
                {
                    return;
                }

                if (_connection != null)
                {
                    _logger.LogTrace("Disposing connection");
                    _connection.Dispose();
                }

                _disposed = true;
            }
        }

        public Task ScriptEvaluateAsync(int database, string script, string key, byte[] messageArguments)
        {
            if (_connection == null)
            {
                throw new InvalidOperationException("Resources.Error_RedisConnectionNotStarted");
            }

            var keys = new RedisKey[] { key };

            var arguments = new RedisValue[] { messageArguments };

            return _connection.GetDatabase(database).ScriptEvaluateAsync(script,
                keys,
                arguments);
        }

        public async Task RestoreLatestValueForKey(int database, string key)
        {
            try
            {
                // Workaround for StackExchange.Redis/issues/61 that sometimes Redis connection is not connected in ConnectionRestored event
                while (!_connection.GetDatabase(database).IsConnected(key))
                {
                    await Task.Delay(200);
                }

                //var redisResult = await _connection.GetDatabase(database).ScriptEvaluateAsync(
                //   @"local newvalue = tonumber(redis.call('GET', KEYS[1]))
                //     if not newvalue or tonumber(newvalue) < tonumber(ARGV[1]) then
                //         return redis.call('SET', KEYS[1], ARGV[1])
                //     else
                //         return nil
                //     end",
                //   new RedisKey[] { key },
                //   new RedisValue[] { _latestMessageId });

                //if (!redisResult.IsNull)
                //{
                //    _logger.LogInformation("Restored Redis Key {0} to the latest Value {1} ", key, _latestMessageId);
                //}
            }
            catch (Exception ex)
            {
                _logger.LogError("Error while restoring Redis Key to the latest Value: " + ex);
            }
        }

        public event Action<Exception> ConnectionFailed;

        public event Action<Exception> ConnectionRestored;

        public event Action<Exception> ErrorMessage;

        private void OnConnectionFailed(object sender, ConnectionFailedEventArgs args)
        {
            _logger.LogWarning(args.ConnectionType.ToString() + " Connection failed. Reason: " + args.FailureType.ToString() + " Exception: " + args.Exception.ToString());
            var handler = ConnectionFailed;
            handler(args.Exception);
        }

        private void OnConnectionRestored(object sender, ConnectionFailedEventArgs args)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation(args.ConnectionType.ToString() + " Connection restored. Reason: " + args.FailureType.ToString() + " Exception: " + (args.Exception?.ToString() ?? "<none>"));
            }
            var handler = ConnectionRestored;
            handler(args.Exception);
        }

        private void OnError(object sender, RedisErrorEventArgs args)
        {
            _logger.LogWarning("Redis Error: " + args.Message);
            var handler = ErrorMessage;
            handler(new InvalidOperationException(args.Message));
        }
    }
}