using System;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace Polly.Extensions.Distributed.DistributedBulkhead
{
    public class RedisDistributedBulkheadBackplane : IDistributedBulkheadBackplane
    {
        private readonly IConnectionMultiplexer _multiplexer;
        
        private IDatabase _database => _multiplexer.GetDatabase();
        private static string UsedCountKey(string bulkheadKey) => $"{bulkheadKey}_BulkheadUsedCount";

        public RedisDistributedBulkheadBackplane(IConnectionMultiplexer multiplexer)
        {
            _multiplexer = multiplexer ?? throw new ArgumentNullException(nameof(multiplexer));
        }
        
        public long GetCurrentUsedCount(string bulkheadKey)
        {
            return _database.StringGet(UsedCountKey(bulkheadKey)).TryParse(out long currentUsedCount) 
                ? currentUsedCount 
                : 0;
        }

        public async Task<long> GetCurrentUsedCountAsync(string bulkheadKey)
        {
            var currentCount = await _database.StringGetAsync(UsedCountKey(bulkheadKey));
            
            return currentCount.TryParse(out long currentUsedCount) 
                ? currentUsedCount 
                : 0;
        }

        public Task<long> IncrementCurrentUsedCountAsync(string bulkheadKey, int count = 1)
        {
            return _database.StringIncrementAsync(UsedCountKey(bulkheadKey), count);
        }

        public Task<long> DecrementCurrentUsedCountAsync(string bulkheadKey, int count = 1)
        {
            return _database.StringDecrementAsync(UsedCountKey(bulkheadKey), count);
        }

        public async Task<T> WithLeaseAsync<T>(string bulkheadKey, Func<Task<T>> func)
        {
            try
            {
                await IncrementCurrentUsedCountAsync(bulkheadKey);
                return await func();
            }
            finally
            {
                await DecrementCurrentUsedCountAsync(bulkheadKey);
            }
        }
    }
}