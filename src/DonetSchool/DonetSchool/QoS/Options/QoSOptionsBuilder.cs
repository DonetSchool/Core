using static DonetSchool.QoS.Delegates.QoSEvents;

namespace DonetSchool.QoS.Options
{
    public class QoSOptionsBuilder
    {
        public RuleResetEventDelegate RuleResetEvent { get; set; }

        public QoSOnBreakDelegate OnBreakEvent { get; set; }

        public QoSOnResetDelegate OnResetEvent { get; set; }

        public QoSOnHalfOpenDelegate OnHalfOpen { get; set; }

        public LimitProcessResultListener OnLimitProcessResult { get; set; }

        public FallbackActionDelegate OnFallbackAction { get; set; }

        public OnFallbackDelegate OnFallback { get; set; }
    }
}