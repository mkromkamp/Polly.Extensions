using System;
using System.Threading.Tasks;
using Polly.Utilities;
using StackExchange.Redis;

namespace Polly.Extensions.Distributed.DistributedBulkhead
{
    public static class PolicyBuilderExtensions
    {
        public static DistributedBulkheadPolicy DistributedBulkhead(this PolicyBuilder builder, IDistributedBulkheadBackplane backplane, string keyPrefix, int maxParallelization)
        {
            Task DoNothingAsync(Context _) => TaskHelper.EmptyTask;
            return builder.DistributedBulkhead(backplane, keyPrefix, maxParallelization, DoNothingAsync);
        }
        
        public static DistributedBulkheadPolicy DistributedBulkhead(this PolicyBuilder builder, IDistributedBulkheadBackplane backplane, string keyPrefix, int maxParallelization, Func<Context, Task> onBulkheadRejected)
        {
            return new DistributedBulkheadPolicy(backplane, keyPrefix, maxParallelization, onBulkheadRejected);
        }
    }
}