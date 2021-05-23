using System;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Services
{
    public interface ICounterStoreService
    {
        Task<(bool IsSuccess, TCounter Counter)> TryGetCounterAsync<TCounter>(string key);

        Task SetCounterAsync<TCounter>(string key, TCounter counter, TimeSpan absoluteExpirationRelativeToNow);
    }
}