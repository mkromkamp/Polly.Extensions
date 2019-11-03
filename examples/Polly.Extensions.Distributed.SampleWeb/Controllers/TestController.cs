using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Polly.Extensions.Distributed.DistributedBulkhead;

namespace Polly.Extensions.SampleWeb.Controllers
{
    [Route("test")]
    public class TestController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IDistributedBulkheadBackplane _bulkheadBackplane;

        private AsyncPolicy _distributedBulkhead;

        public TestController(ILogger<TestController> logger, IDistributedBulkheadBackplane bulkheadBackplane)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _bulkheadBackplane = bulkheadBackplane ?? throw new ArgumentNullException(nameof(bulkheadBackplane));
            
            
            _distributedBulkhead = Policy
                .Handle<Exception>()
                .DistributedBulkhead(_bulkheadBackplane, "test", 3, OnBulkheadRejected);
        }

        private Task OnBulkheadRejected(Context arg)
        {
            _logger.LogError("Bulkhead rejected", arg.PolicyKey);
            return Task.CompletedTask;
        }

        [HttpGet]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var policyResult = await _distributedBulkhead.ExecuteAndCaptureAsync(async () =>
                {
                    await Task.Delay(15000, cancellationToken);
                    return 4;
                });

            return Ok(policyResult.Outcome.ToString("G"));
        }
    }
}