namespace DonetSchool.QoS.Options
{
    public class QoSOptions
    {
        /// <summary>
        /// 熔断限制
        /// </summary>
        public BreakOptions QoS { get; set; }

        /// <summary>
        /// 限流配置
        /// </summary>
        public QuotaOptions Quota { get; set; }
    }
}