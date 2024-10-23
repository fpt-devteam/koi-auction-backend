using BiddingService.IServices;
using Microsoft.Extensions.Caching.Memory;

namespace BiddingService.Services
{

    public class MemoryCacheService : ICacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        // Định nghĩa SetBalance
        public void SetBalance(int userId, int balance, TimeSpan expiration)
        {
            _memoryCache.Set(userId, balance, expiration);
        }

        // Định nghĩa RemoveBalance
        public void RemoveBalance(int userId)
        {
            _memoryCache.Remove(userId);
        }

        // Định nghĩa GetBalance (để truy xuất)
        public int GetBalance(int userId)
        {
            if (_memoryCache.TryGetValue(userId, out int balance))
            {
                return balance;
            }
            return 0;
        }

        public bool IsUserBalanceInCache(int userId)
        {
            return _memoryCache.TryGetValue(userId, out _);
        }

    }

}
