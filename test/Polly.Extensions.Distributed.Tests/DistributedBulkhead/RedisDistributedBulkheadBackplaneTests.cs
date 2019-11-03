using System;
using Moq;
using Polly.Extensions.Distributed.DistributedBulkhead;
using StackExchange.Redis;
using Xunit;

namespace Polly.Extensions.Tests.DistributedBulkhead
{
    public class RedisDistributedBulkheadBackplaneTests
    {
        private readonly IConnectionMultiplexer _multiplexer;

        private readonly IDistributedBulkheadBackplane _backplane;
        
        public RedisDistributedBulkheadBackplaneTests()
        {
            _multiplexer = Mock.Of<IConnectionMultiplexer>();
            
            _backplane = new RedisDistributedBulkheadBackplane(_multiplexer);
        }

        [Fact]
        public void GivenNullMultiplexerWhenConstructingShouldThrowArgumentNullException()
        {
            // Given, When, Then
            Assert.Throws<ArgumentNullException>(() => new RedisDistributedBulkheadBackplane(null));
        }
    }
}