using DonetSchool.QoS.Config;
using DonetSchool.QoS.Counters;
using DonetSchool.QoS.Services;
using System;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement.Handlers
{
    public class FixedWindowHandler : BaseQuotaHandler, IQuotaHandler
    {
        private readonly ICounterStoreService _counterStoreService;

        public FixedWindowHandler(ICounterStoreService counterStoreService, ILockerService lockerService) : base(lockerService)
        {
            _counterStoreService = counterStoreService;
        }

        public LimitRuleType RuleType => LimitRuleType.FixedWindow;

        public async Task<(bool isAllow, int waittimeMills)> IsAllowRequestAsync(string requestIdentity, QuotaConfig config)
        {
            using (await GetLockerAsync(requestIdentity, config.Locker))
            {
                var currentTime = DateTime.UtcNow;
                var startMinTime = new DateTime(1970, 1, 1, 0, 0, 0);
                //2*60*1000
                var periodMillSeconds = config.PeriodTimeSpan.TotalMilliseconds;
                var windowStartTime = startMinTime.AddMilliseconds(MathFloor((currentTime - startMinTime).TotalMilliseconds, periodMillSeconds) * periodMillSeconds);
                //获取对应的窗口计数器
                var (IsSuccess, Counter) = await _counterStoreService.TryGetCounterAsync<WindowCounter>($"{RuleType}:{requestIdentity}");

                if (!IsSuccess || Counter.StartTime < windowStartTime)
                {
                    Counter = new WindowCounter(windowStartTime, config.PeriodCount, config.PeriodTimeSpan);
                }
                if (Counter.Count >= Counter.LimitPeriodCount)
                {
                    return (false, 0);
                }
                Counter.Count++;
                await _counterStoreService.SetCounterAsync($"{RuleType}:{requestIdentity}", Counter, TimeSpan.FromMilliseconds(Counter.LimitPeriodMillSeconds + 10000));
                return (true, 0);
            }
        }
    }
}