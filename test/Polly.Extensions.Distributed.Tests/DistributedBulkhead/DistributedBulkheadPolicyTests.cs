using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Polly.Bulkhead;
using Polly.Extensions.Distributed.DistributedBulkhead;
using Polly.Utilities;
using Shouldly;
using Xunit;

namespace Polly.Extensions.Tests.DistributedBulkhead
{
    public class DistributedBulkheadPolicyTests
    {
        private readonly IDistributedBulkheadBackplane _backplane;
        private readonly string _keyPrefix;
        private readonly int _maxParallelization;
        private readonly Func<Context, Task> _onBulkheadRejected;
        
        private readonly DistributedBulkheadPolicy _policy;
        
        public DistributedBulkheadPolicyTests()
        {
            _backplane = Mock.Of<IDistributedBulkheadBackplane>();
            _keyPrefix = "test";
            _maxParallelization = 3;
            _onBulkheadRejected = Mock.Of<Func<Context, Task>>(); 

            _policy = new DistributedBulkheadPolicy(_backplane, _keyPrefix, _maxParallelization, _onBulkheadRejected);
        }

        [Fact]
        public void GivenNullBackplaneWhenConstructingShouldThrowArgumentNullException()
        {
            // Given, When, Then
            Assert.Throws<ArgumentNullException>(() =>
                new DistributedBulkheadPolicy(null, _keyPrefix, _maxParallelization, _onBulkheadRejected));
        }
        
        [Fact]
        public void GivenNullEmptyKeyPrefixWhenConstructingShouldThrowArgumentException()
        {
            // Given, When, Then
            Assert.Throws<ArgumentException>(() =>
                new DistributedBulkheadPolicy(_backplane, string.Empty, _maxParallelization, _onBulkheadRejected));
        }
        
        [Fact]
        public void GivenNullZeroMaxParallelizationWhenConstructingShouldThrowArgumentException()
        {
            // Given, When, Then
            Assert.Throws<ArgumentException>(() =>
                new DistributedBulkheadPolicy(_backplane, _keyPrefix, 0, _onBulkheadRejected));
        }

        [Fact]
        public async Task GivenMaxParralelizationIsNotHitWhenExecutingShouldReturnResult()
        {
            // Given
            Mock.Get(_backplane)
                .Setup(x => x.IncrementCurrentUsedCountAsync(_keyPrefix, 1, CancellationToken.None))
                .ReturnsAsync(1);
            
            Mock.Get(_backplane)
                .Setup(x => x.DecrementCurrentUsedCountAsync(_keyPrefix, 1, CancellationToken.None))
                .ReturnsAsync(1);

            var testTask = TaskHelper.EmptyTask;

            // When
            var result = await _policy.ExecuteAndCaptureAsync(() => testTask);

            // Then
            result.Outcome.ShouldBe(OutcomeType.Successful);
            Mock.Get(_backplane)
                .Verify(x => x.DecrementCurrentUsedCountAsync(_keyPrefix, 1, CancellationToken.None),
                    Times.Once());
        }
        
        [Fact]
        public async Task GivenMaxParralelizationIsHitWhenExecutingShouldReturnBulkHeadRejectedAsync()
        {
            // Given
            Mock.Get(_backplane)
                .Setup(x => x.IncrementCurrentUsedCountAsync(_keyPrefix, 1, CancellationToken.None))
                .ReturnsAsync(_maxParallelization + 1);
            
            Mock.Get(_backplane)
                .Setup(x => x.DecrementCurrentUsedCountAsync(_keyPrefix, 1, CancellationToken.None))
                .ReturnsAsync(1);

            var testTask = TaskHelper.EmptyTask;

            // When
            var result = await _policy.ExecuteAndCaptureAsync(() => testTask);

            // Then
            result.Outcome.ShouldBe(OutcomeType.Failure);
            result.FinalException.ShouldBeOfType<BulkheadRejectedException>();
            Mock.Get(_backplane)
                .Verify(x => x.DecrementCurrentUsedCountAsync(_keyPrefix, 1, CancellationToken.None),
                    Times.Once());
        }
    }
}