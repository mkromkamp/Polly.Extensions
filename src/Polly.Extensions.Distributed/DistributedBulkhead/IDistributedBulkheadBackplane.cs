using System;
using System.Threading.Tasks;

namespace Polly.Extensions.Distributed.DistributedBulkhead
{
    public interface IDistributedBulkheadBackplane
    {
        /// <summary>
        /// Return the currently used bulkhead count.
        /// </summary>
        long GetCurrentUsedCount(string bulkheadKey);
        
        /// <summary>
        /// Return the currently used bulkhead count.
        /// </summary>
        Task<long> GetCurrentUsedCountAsync(string bulkheadKey);
        
        /// <summary>
        /// Increment currently used bulkhead count.
        /// </summary>
        Task<long> IncrementCurrentUsedCountAsync(string bulkheadKey, int count = 1);
        
        /// <summary>
        /// Return the currently used bulkhead count.
        /// </summary>
        Task<long> DecrementCurrentUsedCountAsync(string bulkheadKey, int count = 1);
        
        /// <summary>
        /// Lease on bulkhead action while performing the task.
        /// </summary>
        Task<T> WithLeaseAsync<T>(string bulkheadKey, Func<Task<T>> func);
    }
}