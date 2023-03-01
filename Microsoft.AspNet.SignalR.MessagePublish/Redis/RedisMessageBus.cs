// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Redis
{
    /// <summary>
    /// Uses Redis pub-sub to scale-out SignalR applications in web farms.
    /// </summary>
    public class RedisMessageBus : ScaleoutMessageBus
    {
        private readonly ILogger _logger;

        private readonly int _db;
        private readonly string _key;

        private IRedisConnection _connection;
        private string _connectionString;
        private readonly object _callbackLock = new object();
        //private ulong? lastId = null;

        public RedisMessageBus(RedisScaleoutConfiguration configuration, IRedisConnection connection, ILoggerFactory loggerFactory)
            : base(configuration)
        {
            _connection = connection;
            _connection.ConnectionFailed += OnConnectionFailed;
            _connection.ConnectionRestored += OnConnectionRestored;
            _connection.ErrorMessage += OnConnectionError;

            _connectionString = configuration.ConnectionString;
            _db = configuration.Database;
            _key = configuration.EventKey;
            _logger = loggerFactory.CreateLogger<RedisMessageBus>();

            ReconnectDelay = TimeSpan.FromSeconds(2);

            ThreadPool.QueueUserWorkItem(_ =>
            {
                var ignore = ConnectWithRetry();
            });
        }

        public TimeSpan ReconnectDelay { get; set; }

        public virtual void OpenStream(int streamIndex)
        {
        }

        protected override Task Send(int streamIndex, IList<Message> messages)
        {
            return _connection.ScriptEvaluateAsync(
                _db,
                @"local newId = redis.call('INCR', KEYS[1])
                  local payload = newId .. ' ' .. ARGV[1]
                  redis.call('PUBLISH', KEYS[1], payload)
                  return {newId, ARGV[1], payload}",
                _key,
                RedisMessage.ToBytes(messages));
        }

        protected override void Dispose(bool disposing)
        {
            _logger.LogInformation(nameof(RedisMessageBus) + " is being disposed");
            if (disposing)
            {
                Shutdown();
            }

            base.Dispose(disposing);
        }

        private void Shutdown()
        {
            if (_connection != null)
            {
                _connection.Close(_key, allowCommandsToComplete: false);
            }
        }

        private void OnConnectionFailed(Exception ex)
        {
            string errorMessage = (ex != null) ? ex.Message : "Resources.Error_RedisConnectionClosed";

            _logger.LogInformation("OnConnectionFailed - " + errorMessage);
        }

        private void OnConnectionError(Exception ex)
        {
        }

        private async void OnConnectionRestored(Exception ex)
        {
            await _connection.RestoreLatestValueForKey(_db, _key);

            OpenStream(0);
        }

        internal async Task ConnectWithRetry()
        {
            while (true)
            {
                try
                {
                    await ConnectToRedisAsync();

                    OpenStream(0);

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error connecting to Redis - " + ex.GetBaseException());
                }

                await Task.Delay(ReconnectDelay);
            }
        }

        private Task ConnectToRedisAsync()
        {
            // We need to hold the dispose lock during this in order to ensure that ConnectAsync completes fully without Dispose getting in the way
            return _connection.ConnectAsync(_connectionString, null);
        }
    }
}