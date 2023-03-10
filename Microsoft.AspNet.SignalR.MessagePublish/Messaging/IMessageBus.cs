// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Messaging
{
    public interface IMessageBus
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        Task Publish(Message message);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="subscriber"></param>
        /// <param name="cursor"></param>
        /// <param name="callback"></param>
        /// <param name="maxMessages"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        //IDisposable Subscribe(ISubscriber subscriber, string cursor, Func<MessageResult, object, Task<bool>> callback, int maxMessages, object state);
    }
}