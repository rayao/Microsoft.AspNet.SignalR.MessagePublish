using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Redis;
using Newtonsoft.Json;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSignalRRedisPublish(this IServiceCollection obj,
            string redisConnectionString, string eventKey)
        {
            return obj.AddSingleton<IHubPipelineInvoker, HubPipeline>()
                .AddSingleton<IMemoryPool, MemoryPool>()

                .AddSingleton<IMessageBus, RedisMessageBus>()
                .AddSingleton<IRedisConnection, RedisConnection>()
                .AddSingleton(new RedisScaleoutConfiguration(redisConnectionString, eventKey))
                .AddSignalRConnectionManagerFactory(sp => serializer =>
                {
                    return ActivatorUtilities.CreateInstance<ConnectionManager>(sp, serializer);
                });
        }
        public static IServiceCollection AddSignalRConnectionManagerFactory(this IServiceCollection obj,
            Func<IServiceProvider, Func<JsonSerializer, ConnectionManager>> factory)
        {
            return obj.AddSingleton<Func<JsonSerializer, ConnectionManager>>(factory);
        }

        public static ConnectionManager CreateConnectionManager(this IServiceProvider obj, JsonSerializer serializer)
        {
            return ActivatorUtilities.CreateInstance<ConnectionManager>(obj, serializer);
        }
    }
}
