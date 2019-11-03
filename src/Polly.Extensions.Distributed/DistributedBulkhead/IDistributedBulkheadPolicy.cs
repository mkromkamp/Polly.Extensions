using System;
using Polly.Bulkhead;

namespace Polly.Extensions.Distributed.DistributedBulkhead
{
    public interface IDistributedBulkheadPolicy : IBulkheadPolicy
    {
    }
    
    public interface IDistributedBulkheadPolicy<TResult> : IDistributedBulkheadPolicy
    {
    }
}