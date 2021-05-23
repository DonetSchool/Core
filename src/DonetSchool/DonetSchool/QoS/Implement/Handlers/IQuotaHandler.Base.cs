using DonetSchool.QoS.Config;
using DonetSchool.QoS.Exceptions;
using DonetSchool.QoS.Services;
using System;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Implement.Handlers
{
    public abstract class BaseQuotaHandler
    {
        private readonly ILockerService _lockerService;

        protected BaseQuotaHandler(ILockerService lockerService)
        {
            _lockerService = lockerService;
        }

        protected async Task<IDisposable> GetLockerAsync(string requestIdentity, LockerConfig lockerConfig)
        {
            var (IsSuccess, Locker) = await _lockerService.TryTakeLockAsync(requestIdentity, lockerConfig);
            if (!IsSuccess)
            {
                throw new ThrottlException();
            }
            return Locker;
        }

        protected int MathFloor(double divisor, double dividend)
        {
            if (dividend == 0)
            {
                return (int)Math.Floor(divisor);
            }
            return (int)Math.Floor(divisor / dividend);
        }

        protected DateTime GetLastMaxTime(QuotaConfig config)
        {
            var currentTime = DateTime.UtcNow;
            var startMinTime = new DateTime(1970, 1, 1, 0, 0, 0);
            //2*60*1000
            var periodMillSeconds = config.PeriodTimeSpan.TotalMilliseconds;
            var startTime = startMinTime.AddMilliseconds(MathFloor((currentTime - startMinTime).TotalMilliseconds, periodMillSeconds) * periodMillSeconds);

            return startTime;
        }
    }
}