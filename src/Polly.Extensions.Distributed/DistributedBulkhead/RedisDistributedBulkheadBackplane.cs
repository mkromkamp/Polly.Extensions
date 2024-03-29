using System;
using System.Threading;
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

        public async Task<long> GetCurrentUsedCountAsync(string bulkheadKey, CancellationToken cancellationToken = default)
        {
            var currentCount = await _database.StringGetAsync(UsedCountKey(bulkheadKey));
            
            return currentCount.TryParse(out long currentUsedCount) 
                ? currentUsedCount 
                : 0;
        }

        public Task<long> IncrementCurrentUsedCountAsync(string bulkheadKey, int count = 1, CancellationToken cancellationToken = default)
        {
            return _database.StringIncrementAsync(UsedCountKey(bulkheadKey), count);
        }

        public Task<long> DecrementCurrentUsedCountAsync(string bulkheadKey, int count = 1, CancellationToken cancellationToken = default)
        {
            return _database.StringDecrementAsync(UsedCountKey(bulkheadKey), count);
        }

        public async Task<TResult> WithLeaseAsync<TResult>(string bulkheadKey, int count, Func<long, Task<TResult>> func, CancellationToken cancellationToken = default)
        {
            try
            {
                var currentUsed = await IncrementCurrentUsedCountAsync(bulkheadKey, count, cancellationToken);
                return await func(currentUsed);
            }
            finally
            {
                await DecrementCurrentUsedCountAsync(bulkheadKey, count, cancellationToken);
            }
        }
    }
}