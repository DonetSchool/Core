using DonetSchool.QoS.Config;
using DonetSchool.QoS.Counters;
using DonetSchool.QoS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement.Handlers
{
    public class SlidingWindowQuotaHandler : BaseQuotaHandler, IQuotaHandler
    {
        private readonly ICounterStoreService _counterStoreService;

        public SlidingWindowQuotaHandler(ILockerService lockerService, ICounterStoreService counterStoreService) : base(lockerService)
        {
            _counterStoreService = counterStoreService;
        }

        public LimitRuleType RuleType => LimitRuleType.SlidingWindow;

        public async Task<(bool isAllow, int waittimeMills)> IsAllowRequestAsync(string requestIdentity, QuotaConfig config)
        {
            using (await GetLockerAsync(requestIdentity, config.Locker))
            {
                var startMinTime = new DateTime(1970, 1, 1, 0, 0, 0);
                var periodArray = GetWindowPeriodArray(startMinTime, config.PeriodTimeSpan, config.Count);
                var sumCount = 0;
                WindowCounter currentCounter = null;
                foreach (var item in periodArray)
                {
                    var (IsSuccess, Counter) = await _counterStoreService.TryGetCounterAsync<WindowCounter>($"{RuleType}:{item}:{requestIdentity}");
                    if (!IsSuccess)
                    {
                        Counter = new WindowCounter(startMinTime.AddMilliseconds(item), config.PeriodCount, config.PeriodTimeSpan);
                    }
                    sumCount += Counter.Count;
                    if (currentCounter == null || Counter.StartTime > currentCounter.StartTime)
                    {
                        currentCounter = Counter;
                    }
                }
                if (sumCount >= config.PeriodCount)
                {
                    return (false, 0);
                }
                currentCounter.Count++;
                await _counterStoreService.SetCounterAsync($"{RuleType}:{periodArray.Max()}:{requestIdentity}", currentCounter, TimeSpan.FromMilliseconds(currentCounter.LimitPeriodMillSeconds + 10000));
                return (true, 0);
            }
        }

        private long[] GetWindowPeriodArray(DateTime startTime, TimeSpan periodTimeSpan, int slideCount)
        {
            var periodMillSeconds = periodTimeSpan.TotalMilliseconds;
            var everyTimePeriod = periodMillSeconds / slideCount;
            var currentTime = DateTime.UtcNow;
            var array = new long[slideCount];
            for (int i = 0; i < slideCount; i++)
            {
                var time = startTime.AddMilliseconds((Math.Floor((currentTime - startTime).TotalMilliseconds / everyTimePeriod) - i) * everyTimePeriod);
                array[i] = (long)(time - startTime).TotalMilliseconds;
            }
            return array;
        }
    }
}