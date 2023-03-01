// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Messaging;
using Newtonsoft.Json;
using System;

namespace Microsoft.AspNet.SignalR.Infrastructure
{
    /// <summary>
    /// Default <see cref="IConnectionManager"/> implementation.
    /// </summary>
    public class ConnectionManager
    {
        private readonly IMessageBus _bus;
        private readonly IMemoryPool _memoryPool;
        private readonly IHubPipelineInvoker _invoker;
        private readonly JsonSerializer _serializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionManager"/> class.
        /// </summary>
        /// <param name="resolver">The <see cref="IDependencyResolver"/>.</param>
        public ConnectionManager(IMessageBus bus, IMemoryPool memoryPool, IHubPipelineInvoker invoker, JsonSerializer serializer)
        {
            _bus = bus;
            _memoryPool = memoryPool;
            _invoker = invoker;
            _serializer = serializer;
        }

        /// <summary>
        /// Returns a <see cref="IHubContext"/> for the specified <see cref="IHub"/>.
        /// </summary>
        /// <typeparam name="T">Type of the <see cref="IHub"/></typeparam>
        /// <returns>a <see cref="IHubContext"/> for the specified <see cref="IHub"/></returns>
        public IHubContext GetHubContext<T>()
        {
            return GetHubContext(typeof(T).GetHubName());
        }

        /// <summary>
        /// Returns a <see cref="IHubContext"/>for the specified hub.
        /// </summary>
        /// <param name="hubName">Name of the hub</param>
        /// <returns>a <see cref="IHubContext"/> for the specified hub</returns>
        public IHubContext GetHubContext(string hubName)
        {
            var connection = GetConnectionCore(connectionName: null);
            //var hubManager = _resolver.Resolve<IHubManager>();
            var pipelineInvoker = _invoker;

            //hubManager.EnsureHub(hubName,
            //    _counters.ErrorsHubResolutionTotal,
            //    _counters.ErrorsHubResolutionPerSec,
            //    _counters.ErrorsAllTotal,
            //    _counters.ErrorsAllPerSec);

            return new HubContext(connection, pipelineInvoker, hubName);
        }

        internal Connection GetConnectionCore(string connectionName)
        {
            //IList<string> signals = connectionName == null ? Array.Empty<string>() : new[] { connectionName };

            // Ensure that this server is listening for any ACKs sent over the bus.
            // This is important in case there are any calls to Groups.Add on a context.
            //_resolver.Resolve<AckSubscriber>();

            // Give this a unique id
            var connectionId = Guid.NewGuid().ToString();
            return new Connection(_bus, _serializer, connectionName, connectionId, _memoryPool);
        }
    }
}