using DonetSchool.QoS.Config;
using Polly;
using Polly.CircuitBreaker;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Delegates
{
    public static class QoSEvents
    {
        public delegate void RuleResetEventDelegate(List<QoSConfig> qoSConfigs);

        public static RuleResetEventDelegate RuleResetEvent;

        public delegate void QoSOnBreakDelegate(Exception ex, CircuitState circuitState, TimeSpan timeSpan, Context context);

        public static QoSOnBreakDelegate OnBreakEvent;

        public delegate void QoSOnResetDelegate(Context context);

        public static QoSOnResetDelegate OnResetEvent;

        public delegate void QoSOnHalfOpenDelegate();

        public static QoSOnHalfOpenDelegate OnHalfOpen;

        public delegate void LimitProcessResultListener(string requestIdentity, QuotaConfig quotaConfig, bool isAllow, int waittimeMills);

        public static LimitProcessResultListener OnLimitProcessResult;

        public delegate Task FallbackActionDelegate(Exception exception, Context keyValuePairs, CancellationToken cancellationToken);

        public static FallbackActionDelegate OnFallbackAction;

        public delegate Task OnFallbackDelegate(Exception exception, Context keyValuePairs);

        public static OnFallbackDelegate OnFallback;
    }
}