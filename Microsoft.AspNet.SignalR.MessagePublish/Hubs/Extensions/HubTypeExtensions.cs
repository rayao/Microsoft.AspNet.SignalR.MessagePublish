// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal static class HubTypeExtensions
    {
        internal static string GetHubName(this Type type)
        {
            return GetHubTypeName(type);
        }

        private static string GetHubTypeName(Type type)
        {
            var lastIndexOfBacktick = type.Name.LastIndexOf('`');
            if (lastIndexOfBacktick == -1)
            {
                return type.Name;
            }
            else
            {
                return type.Name.Substring(0, lastIndexOfBacktick);
            }
        }
    }
}