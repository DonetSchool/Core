using System;

namespace DonetSchool.QoS.Counters
{
    public class BucketCounter : QuotaCounter
    {
        public BucketCounter()
        {
        }

        public BucketCounter(int capacity, int inflowPerUnit, TimeSpan inflowUnit) : this()
        {
            if (capacity < 1)
            {
                throw new ArgumentException("the capacity can not less than 1.");
            }

            if (inflowPerUnit < 1)
            {
                throw new ArgumentException("the inflow quantity per unit can not less than 1.");
            }

            if (inflowUnit.TotalMilliseconds < 1)
            {
                throw new ArgumentException("the inflow unit can not less than 1ms.");
            }

            Capacity = capacity;
            LimitPeriodMillSeconds = (long)inflowUnit.TotalMilliseconds;
            LimitPeriodCount = inflowPerUnit;
            Count = capacity;
        }

        /// <summary>
        /// 初始数量
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// 最后一次访问时间
        /// </summary>
        public DateTime LastTime { get; set; }

        /// <summary>
        /// 令牌数量
        /// </summary>
        public int Count { get; set; }
    }
}