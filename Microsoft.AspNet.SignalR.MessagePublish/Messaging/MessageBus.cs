// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Messaging
{
    /// <summary>
    /// 
    /// </summary>
    public class MessageBus : IMessageBus, IDisposable
    {
        public MessageBus()
        {
        }

        /// <summary>
        /// Publishes a new message to the specified event on the bus.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        public virtual Task Publish(Message message)
        {
            throw new NotImplementedException();
        }

        protected virtual void Dispose(bool disposing)
        {
        }

        public void Dispose()
        {
            Dispose(true);
        }
    }
}