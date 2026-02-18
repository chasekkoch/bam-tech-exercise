using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;

namespace StargateAPI.Health
{
    public class OpenSearchHealthCheck : IHealthCheck
    {
        private readonly IOpenSearchClient _client;
        private readonly bool _enabled;

        public OpenSearchHealthCheck(IOpenSearchClient client, bool enabled)
        {
            _client = client;
            _enabled = enabled;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            if (!_enabled)
            {
                return HealthCheckResult.Healthy("OpenSearch is disabled");
            }

            try
            {
                var response = await _client.PingAsync(ct: cancellationToken);

                if (response.IsValid)
                {
                    var data = new Dictionary<string, object>
                    {
                        { "cluster", "connected" }
                    };

                    return HealthCheckResult.Healthy("OpenSearch is healthy", data);
                }

                return HealthCheckResult.Unhealthy(
                    $"OpenSearch ping failed: {response.ServerError?.Error?.Reason ?? "Unknown error"}");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy(
                    "OpenSearch is unreachable",
                    ex);
            }
        }
    }
}
