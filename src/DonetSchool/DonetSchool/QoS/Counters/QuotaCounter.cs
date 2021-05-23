using System;

namespace DonetSchool.QoS.Counters
{
    public class QuotaCounter
    {
        /// <summary>
        /// 计数器唯一标识
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        /// <summary>
        /// 限流时间间隔
        /// </summary>
        public long LimitPeriodMillSeconds { get; set; }

        /// <summary>
        /// 限流时间段内而可通过数量
        /// </summary>
        public int LimitPeriodCount { get; set; }
    }
}