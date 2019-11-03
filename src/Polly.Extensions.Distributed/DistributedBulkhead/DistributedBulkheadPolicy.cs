using System;
using System.Threading;
using System.Threading.Tasks;
using Polly.Bulkhead;
using StackExchange.Redis;

namespace Polly.Extensions.Distributed.DistributedBulkhead
{
    public class DistributedBulkheadPolicy : AsyncPolicy, IDistributedBulkheadPolicy
    {
        private bool _isDisposed;
        
        private readonly int _maxParallelization;
        private readonly Func<Context, Task> _onBulkheadRejectedAsync;
        private readonly IDistributedBulkheadBackplane _backplane;
        private readonly string _keyPrefix;

        public DistributedBulkheadPolicy(IDistributedBulkheadBackplane backplane, string keyPrefix, int maxParallelization, Func<Context, Task> onBulkheadRejectedAsync, PolicyBuilder policyBuilder = null) 
            : base(policyBuilder)
        {
            if (string.IsNullOrWhiteSpace(keyPrefix)) throw new ArgumentException(nameof(keyPrefix));
            if (maxParallelization <= 0) throw new ArgumentException("Max parallelization should be at least one", nameof(maxParallelization));
            
            _backplane = backplane ?? throw new ArgumentNullException(nameof(backplane));
            _keyPrefix = keyPrefix;
            _onBulkheadRejectedAsync = onBulkheadRejectedAsync;
            _maxParallelization = maxParallelization;
        }

        protected override async Task<TResult> ImplementationAsync<TResult>(Func<Context, CancellationToken, Task<TResult>> action, Context context, CancellationToken cancellationToken,
            bool continueOnCapturedContext)
        {
            try
            {
                var usedCount = await _backplane.IncrementCurrentUsedCountAsync(_keyPrefix).ConfigureAwait(continueOnCapturedContext);
                if (usedCount > _maxParallelization)
                {
                    await _onBulkheadRejectedAsync(context).ConfigureAwait(continueOnCapturedContext);
                    throw new BulkheadRejectedException();
                }

                return await action(context, cancellationToken).ConfigureAwait(continueOnCapturedContext);
            }
            finally
            {
                await _backplane.DecrementCurrentUsedCountAsync(_keyPrefix).ConfigureAwait(continueOnCapturedContext);
            }
        }
        
        public int BulkheadAvailableCount => (int) _backplane.GetCurrentUsedCount(_keyPrefix);

        public int QueueAvailableCount => 0;

        public void Dispose()
        {
            if (_isDisposed)
                return;
            
            _isDisposed = true;
        }
    }
}