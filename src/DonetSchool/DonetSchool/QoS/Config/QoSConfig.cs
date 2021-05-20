namespace DonetSchool.QoS.Config
{
    public class QoSConfig
    {
        /// <summary>
        /// 匹配规则
        /// </summary>
        public RuleMatchConfig MatchConfig { get; set; }

        /// <summary>
        /// 熔断规则
        /// </summary>
        public BreakConfig BreakConfig { get; set; }

        /// <summary>
        /// 限流规则
        /// </summary>
        public QuotaConfig QuotaConfig { get; set; }
    }
}