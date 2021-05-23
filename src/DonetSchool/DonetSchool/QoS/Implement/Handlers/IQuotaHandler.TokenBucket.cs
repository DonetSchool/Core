using DonetSchool.QoS.Config;
using DonetSchool.QoS.Counters;
using DonetSchool.QoS.Services;
using System;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement.Handlers
{
    public class TokenBucketQuotaHandler : BaseQuotaHandler, IQuotaHandler
    {
        private readonly ICounterStoreService _counterStoreService;

        public TokenBucketQuotaHandler(ILockerService lockerService, ICounterStoreService counterStoreService) : base(lockerService)
        {
            _counterStoreService = counterStoreService;
        }

        public LimitRuleType RuleType => LimitRuleType.TokenBucket;

        public async Task<(bool isAllow, int waittimeMills)> IsAllowRequestAsync(string requestIdentity, QuotaConfig config)
        {
            using (await GetLockerAsync(requestIdentity, config.Locker))
            {
                var lastTime = GetLastMaxTime(config);
                var (IsSuccess, Counter) = await _counterStoreService.TryGetCounterAsync<BucketCounter>($"{RuleType}:{requestIdentity}");
                if (!IsSuccess)
                {
                    Counter = new BucketCounter(config.Count, config.PeriodCount, config.PeriodTimeSpan)
                    {
                        LastTime = lastTime
                    };
                }
                if (Counter.LastTime < lastTime)
                {
                    Counter.Count = Math.Min(Counter.Capacity, Counter.Count + MathFloor((lastTime - Counter.LastTime).TotalMilliseconds, config.PeriodTimeSpan.TotalMilliseconds) * config.PeriodCount);
                    Counter.LastTime = lastTime;
                }
                bool isAllow = false;
                if (Counter.Count > 0)
                {
                    isAllow = true;
                    Counter.Count--;
                }
                await _counterStoreService.SetCounterAsync($"{RuleType}:{requestIdentity}", Counter, TimeSpan.FromMilliseconds(Counter.LimitPeriodMillSeconds * (Counter.Capacity / Counter.LimitPeriodCount + 1) + 10000));
                return (isAllow, 0);
            }
        }
    }
}