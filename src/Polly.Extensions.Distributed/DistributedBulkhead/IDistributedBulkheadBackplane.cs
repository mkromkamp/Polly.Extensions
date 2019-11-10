using System;
using System.Threading;
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
        Task<long> GetCurrentUsedCountAsync(string bulkheadKey, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Increment currently used bulkhead count.
        /// </summary>
        Task<long> IncrementCurrentUsedCountAsync(string bulkheadKey, int count = 1, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Return the currently used bulkhead count.
        /// </summary>
        Task<long> DecrementCurrentUsedCountAsync(string bulkheadKey, int count = 1, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Lease on bulkhead action while performing the task.
        /// </summary>
        Task<TResult> WithLeaseAsync<TResult>(string bulkheadKey, int count, Func<long, Task<TResult>> func, CancellationToken cancellationToken = default);
    }
}