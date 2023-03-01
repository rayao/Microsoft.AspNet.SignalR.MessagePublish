// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Messaging
{
    /// <summary>
    /// Common base class for scaleout message bus implementations.
    /// </summary>
    public abstract class ScaleoutMessageBus : MessageBus
    {
        protected ScaleoutMessageBus(ScaleoutConfiguration configuration)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }
        }

        /// <summary>
        /// Sends messages to the backplane
        /// </summary>
        /// <param name="messages">The list of messages to send</param>
        /// <returns></returns>
        protected virtual Task Send(IList<Message> messages)
        {
            // If we're only using a single stream then just send
            return Send(0, messages);
        }

        protected virtual Task Send(int streamIndex, IList<Message> messages)
        {
            throw new NotImplementedException();
        }

        public override Task Publish(Message message)
        {
            // TODO: Implement message batching here
            return Send(new[] { message });
        }
    }
}