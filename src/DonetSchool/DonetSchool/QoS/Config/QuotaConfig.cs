using System;

namespace DonetSchool.QoS.Config
{
    public class QuotaConfig
    {
        /// <summary>
        /// 时间段内可通过数量
        /// </summary>
        public int PeriodCount { get; set; }

        /// <summary>
        /// 限流时间段
        /// </summary>
        public TimeSpan PeriodTimeSpan { get; set; }

        /// <summary>
        /// 初始量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 限流方式
        /// </summary>
        public LimitRuleType RuleType { get; set; }

        /// <summary>
        /// 锁配置信息
        /// </summary>
        public LockerConfig Locker { get; set; }
    }
}