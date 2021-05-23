using System;

namespace DonetSchool.QoS.Counters
{
    public class WindowCounter : QuotaCounter
    {
        public WindowCounter()
        {
        }

        public WindowCounter(DateTime startTime, int inflowPerUnit, TimeSpan inflowUnit) : this()
        {
            if (inflowPerUnit < 1)
            {
                throw new ArgumentException("the inflow quantity per unit can not less than 1.");
            }

            if (inflowUnit.TotalMilliseconds < 1)
            {
                throw new ArgumentException("the inflow unit can not less than 1ms.");
            }

            LimitPeriodMillSeconds = (long)inflowUnit.TotalMilliseconds;
            LimitPeriodCount = inflowPerUnit;
            StartTime = startTime;
            Count = 0;
        }

        /// <summary>
        /// 技术起开始 时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 当前已通过数量
        /// </summary>
        public int Count { get; set; }
    }
}