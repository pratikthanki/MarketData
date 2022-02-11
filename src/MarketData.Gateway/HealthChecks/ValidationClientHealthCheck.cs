using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace MarketData.Gateway.HealthChecks
{
    public class ValidationClientHealthCheck : IHealthCheck
    {
        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context, 
            CancellationToken cancellationToken = new CancellationToken())
        {
            return await Task.FromResult(HealthCheckResult.Healthy("Validation Service is accessible."));
        }
    }
}