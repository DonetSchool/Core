using DonetSchool.QoS.Config;

namespace DonetSchool.QoS.Options
{
    public class QuotaOptions
    {
        public QuotaOptions()
        {
            Locker = new LockerConfig
            {
                LockMillSeconds = 5 * 1000,
                WaitMillSeconds = 2 * 1000
            };
            RuleType = LimitRuleType.TokenBucket;
            Count = 500;
        }

        /// <summary>
        /// 限流控制频率信息  100/2ms(每2毫秒可通过100) 1/1s(没1秒可通过1个) 5/2m(每两分钟可通过5个) 3/1h(每一小时可通过3个) 5/1d(每天可通过5个)
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// 初始量
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 限流方式类型
        /// </summary>
        public LimitRuleType RuleType { get; set; }

        /// <summary>
        /// 锁配置信息
        /// </summary>
        public LockerConfig Locker { get; set; }
    }
}