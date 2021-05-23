using DonetSchool.QoS.Config;
using System;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Services
{
    public interface ILockerService
    {
        Task<(bool IsSuccess, IDisposable Locker)> TryTakeLockAsync(string requestIdentity, LockerConfig lockerConfig);
    }
}