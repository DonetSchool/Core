using DonetSchool.QoS.Config;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Services.Implement
{
    public class MonitorLockerService : ILockerService
    {
        public Task<(bool IsSuccess, IDisposable Locker)> TryTakeLockAsync(string requestIdentity, LockerConfig lockerConfig)
        {
            if (string.IsNullOrWhiteSpace(requestIdentity))
            {
                return Task.FromResult<ValueTuple<bool, IDisposable>>((false, null));
            }
            string lockKey = "MonitorLockerService-" + requestIdentity;
            if (Monitor.TryEnter(lockKey, lockerConfig.WaitMillSeconds))
            {
                return Task.FromResult<ValueTuple<bool, IDisposable>>((true, new MonitorLockerReleaser(lockKey, this)));
            }
            return Task.FromResult<ValueTuple<bool, IDisposable>>((false, null));
        }

        public bool TryRelease(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            Monitor.Exit(key);
            return true;
        }

        public struct MonitorLockerReleaser : IDisposable
        {
            private readonly MonitorLockerService _monitorLockerService;

            public MonitorLockerReleaser(string lockKey, MonitorLockerService monitorLockerService)
            {
                LockKey = lockKey;
                _monitorLockerService = monitorLockerService;
            }

            public string LockKey { get; }

            public void Dispose()
            {
                if (!string.IsNullOrWhiteSpace(LockKey) && _monitorLockerService != null)
                {
                    _monitorLockerService.TryRelease(LockKey);
                }
            }
        }
    }
}