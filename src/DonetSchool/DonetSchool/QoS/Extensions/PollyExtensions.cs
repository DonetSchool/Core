using Microsoft.Extensions.Logging;
using Polly;
using static DonetSchool.QoS.Delegates.QoSEvents;

namespace DonetSchool.QoS.Extensions
{
    public static class PollyExtensions
    {
        public static string GetRequestIdentity(this Polly.Context context)
        {
            if (context != null && context.ContainsKey("RequestIdentity"))
            {
                return context["RequestIdentity"] as string;
            }
            return null;
        }

        public static ILogger GetLogger(this Polly.Context context)
        {
            if (context != null && context.ContainsKey("Logger"))
            {
                return context["Logger"] as ILogger;
            }
            return null;
        }

        public static PolicyBuilder Configure(this PolicyBuilder builder, PolicyBuilderDelegate policyBuilderDelegate)
        {
            if (policyBuilderDelegate != null)
            {
                builder = policyBuilderDelegate.Invoke(builder);
            }
            return builder;
        }
    }
}