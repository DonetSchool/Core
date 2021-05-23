using DonetSchool.QoS.Config;
using Polly;
using System;
using System.Collections.Generic;
using System.Text;

namespace DonetSchool.QoS
{
    public interface IPollyFactory
    {
        IAsyncPolicy Get(string requestIdentity, BreakConfig qosConfig);

        Context CreatePollyContext(string requestIdentity, Dictionary<string, object> contextData = null);
    }
}
