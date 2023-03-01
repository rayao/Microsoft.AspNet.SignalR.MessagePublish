// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    /// <summary>
    /// Handles all communication over the hubs persistent connection.
    /// </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling", Justification = "This dispatcher makes use of many interfaces.")]
    public class HubDispatcher // PersistentConnection
    {
        internal static Task Outgoing(IHubOutgoingInvokerContext context)
        {
            ConnectionMessage message = context.GetConnectionMessage();

            return context.Connection.Send(message);
        }
    }
}