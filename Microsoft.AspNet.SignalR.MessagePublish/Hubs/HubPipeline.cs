using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.SignalR.Hubs
{
    internal class HubPipeline : /*IHubPipeline,*/ IHubPipelineInvoker
    {

        public HubPipeline()
        {
        }

        public Task Send(IHubOutgoingInvokerContext context)
        {
            return HubDispatcher.Outgoing(context);
        }
    }
}