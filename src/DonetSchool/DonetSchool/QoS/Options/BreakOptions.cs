namespace DonetSchool.QoS.Options
{
    public class BreakOptions
    {
        public BreakOptions()
        {
            Timeout = 10_000;
            FailureThreshold = 0.7;
            SamplingDuration = 60;
            MinimumThroughput = 20;
            DurationOfBreak = 60;
        }

        /// <summary>
        /// 单位毫秒 请求超时接口
        /// </summary>
        public int Timeout { get; set; }

        /// <summary>
        /// 0.5 Break on >=50% actions result in handled exceptions...
        /// </summary>
        public double FailureThreshold { get; set; }

        /// <summary>
        /// 单位秒 over any 10 second period 每次计算时间间隔
        /// </summary>
        public int SamplingDuration { get; set; }

        /// <summary>
        /// provided at least 8 actions in the 10 second period.间断内最小通过次数
        /// </summary>
        public int MinimumThroughput { get; set; }

        /// <summary>
        /// Break for 30 seconds.
        /// </summary>
        public int DurationOfBreak { get; set; }
    }
}