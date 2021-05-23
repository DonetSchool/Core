using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;

namespace DonetSchool.QoS.Services.Implement
{
    public class MemoryCounterStoreService : ICounterStoreService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCounterStoreService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public Task SetCounterAsync<TCounter>(string key, TCounter counter, TimeSpan absoluteExpirationRelativeToNow)
        {
            _memoryCache.Set(key, counter, absoluteExpirationRelativeToNow);
            return Task.CompletedTask;
        }

        public Task<(bool IsSuccess, TCounter Counter)> TryGetCounterAsync<TCounter>(string key)
        {
            if (_memoryCache.TryGetValue(key, out TCounter counter))
            {
                return Task.FromResult<ValueTuple<bool, TCounter>>((true, counter));
            }
            return Task.FromResult<ValueTuple<bool, TCounter>>((false, counter));
        }
    }
}