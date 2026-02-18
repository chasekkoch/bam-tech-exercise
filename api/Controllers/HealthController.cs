using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenSearch.Client;
using StargateAPI.Logging;

namespace StargateAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class HealthController : ApiControllerBase
    {
        private readonly HealthCheckService _healthCheckService;
        private readonly ILogService _logService;
        private readonly IOpenSearchClient _openSearchClient;
        private readonly OpenSearchOptions _openSearchOptions;

        public HealthController(
            HealthCheckService healthCheckService, 
            ILogService logService,
            IOpenSearchClient openSearchClient,
            OpenSearchOptions openSearchOptions)
        {
            _healthCheckService = healthCheckService;
            _logService = logService;
            _openSearchClient = openSearchClient;
            _openSearchOptions = openSearchOptions;
        }

        /// <summary>
        /// Get detailed health status of all system components
        /// </summary>
        [AllowAnonymous]
        [HttpGet("status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetHealthStatus()
        {
            var report = await _healthCheckService.CheckHealthAsync();

            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                totalDuration = report.TotalDuration.TotalMilliseconds,
                components = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description ?? "No description available",
                    duration = e.Value.Duration.TotalMilliseconds,
                    tags = e.Value.Tags,
                    exception = e.Value.Exception != null ? new
                    {
                        message = e.Value.Exception.Message,
                        type = e.Value.Exception.GetType().Name
                    } : null,
                    data = e.Value.Data.Count > 0 ? e.Value.Data : null
                }).OrderBy(c => c.name)
            };

            var statusCode = report.Status == HealthStatus.Healthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Simple liveness probe - returns 200 if API is running
        /// </summary>
        [AllowAnonymous]
        [HttpGet("live")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult GetLiveness()
        {
            return Ok(new
            {
                status = "Healthy",
                timestamp = DateTime.UtcNow,
                message = "API is running"
            });
        }

        /// <summary>
        /// Readiness probe - returns 200 if API and all dependencies are ready
        /// </summary>
        [AllowAnonymous]
        [HttpGet("ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetReadiness()
        {
            var report = await _healthCheckService.CheckHealthAsync(
                check => check.Tags.Contains("ready"));

            var response = new
            {
                status = report.Status.ToString(),
                timestamp = DateTime.UtcNow,
                ready = report.Status == HealthStatus.Healthy
            };

            var statusCode = report.Status == HealthStatus.Healthy
                ? StatusCodes.Status200OK
                : StatusCodes.Status503ServiceUnavailable;

            return StatusCode(statusCode, response);
        }

        /// <summary>
        /// Force an exception for testing error handling and logging
        /// </summary>
        [HttpPost("force-exception")]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ForceException()
        {
            var exceptionId = Guid.NewGuid().ToString();
            var timestamp = DateTime.UtcNow;

            // Log the exception to OpenSearch
            var logEntry = new ExceptionLogEntry
            {
                ExceptionId = exceptionId,
                RequestId = HttpContext.TraceIdentifier,
                Method = HttpContext.Request.Method,
                Path = HttpContext.Request.Path,
                QueryString = HttpContext.Request.QueryString.ToString(),
                StatusCode = 500,
                ExceptionType = "TestException",
                Message = "This is a forced exception for testing error handling and logging",
                StackTrace = Environment.StackTrace,
                TimestampUtc = timestamp,
                UserAgent = HttpContext.Request.Headers["User-Agent"].ToString(),
                RemoteIp = HttpContext.Connection.RemoteIpAddress?.ToString()
            };

            await _logService.LogExceptionAsync(logEntry, HttpContext.RequestAborted);

            // Return structured error response with exception ID
            return StatusCode(500, new
            {
                error = "TestException",
                message = "This is a forced exception for testing error handling and logging",
                exceptionId = exceptionId,
                timestamp = timestamp,
                traceId = HttpContext.TraceIdentifier
            });
        }

        /// <summary>
        /// Get trending exceptions aggregated by type
        /// </summary>
        [HttpGet("exceptions/trending")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTrendingExceptions([FromQuery] int hours = 24)
        {
            if (!_openSearchOptions.Enabled)
            {
                return Ok(new { enabled = false, data = new object[] { } });
            }

            try
            {
                var indexPattern = $"{_openSearchOptions.IndexPrefix}-exceptions-*";
                var fromDate = DateTime.UtcNow.AddHours(-hours);

                var response = await _openSearchClient.SearchAsync<ExceptionLogEntry>(s => s
                    .Index(indexPattern)
                    .Size(0)
                    .Query(q => q
                        .DateRange(dr => dr
                            .Field(f => f.TimestampUtc)
                            .GreaterThanOrEquals(fromDate)
                        )
                    )
                    .Aggregations(a => a
                        .Terms("by_exception_type", t => t
                            .Field(f => f.ExceptionType.Suffix("keyword"))
                            .Size(5)
                            .Aggregations(aa => aa
                                .Terms("by_message", tm => tm
                                    .Field(f => f.Message.Suffix("keyword"))
                                    .Size(5)
                                )
                                .TopHits("latest_exception", th => th
                                    .Size(1)
                                    .Sort(so => so
                                        .Descending(f => f.TimestampUtc)
                                    )
                                )
                            )
                        )
                    )
                );

                if (!response.IsValid)
                {
                    return StatusCode(500, new { error = "Failed to query OpenSearch", message = response.DebugInformation });
                }

                var trending = response.Aggregations.Terms("by_exception_type").Buckets.Select(bucket => new
                {
                    exceptionType = bucket.Key,
                    count = bucket.DocCount,
                    messages = bucket.Terms("by_message").Buckets.Select(mb => new
                    {
                        message = mb.Key,
                        count = mb.DocCount
                    }).ToList(),
                    latestException = bucket.TopHits("latest_exception").Documents<ExceptionLogEntry>().FirstOrDefault()
                }).ToList();

                return Ok(new
                {
                    enabled = true,
                    hours = hours,
                    timestamp = DateTime.UtcNow,
                    data = trending
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve trending exceptions", message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated list of exceptions
        /// </summary>
        [HttpGet("exceptions")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExceptions(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int hours = 24)
        {
            if (!_openSearchOptions.Enabled)
            {
                return Ok(new { enabled = false, data = new object[] { }, total = 0, page = 1, pageSize = 20 });
            }

            try
            {
                var indexPattern = $"{_openSearchOptions.IndexPrefix}-exceptions-*";
                var fromDate = DateTime.UtcNow.AddHours(-hours);
                var from = (page - 1) * pageSize;

                var response = await _openSearchClient.SearchAsync<ExceptionLogEntry>(s => s
                    .Index(indexPattern)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .DateRange(dr => dr
                            .Field(f => f.TimestampUtc)
                            .GreaterThanOrEquals(fromDate)
                        )
                    )
                    .Sort(so => so
                        .Descending(f => f.TimestampUtc)
                    )
                );

                if (!response.IsValid)
                {
                    return StatusCode(500, new { error = "Failed to query OpenSearch", message = response.DebugInformation });
                }

                return Ok(new
                {
                    enabled = true,
                    data = response.Documents,
                    total = response.Total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)response.Total / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve exceptions", message = ex.Message });
            }
        }

        /// <summary>
        /// Get endpoint statistics aggregated by path
        /// </summary>
        [HttpGet("requests/stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequestStats([FromQuery] int hours = 24)
        {
            if (!_openSearchOptions.Enabled)
            {
                return Ok(new { enabled = false, data = new object[] { } });
            }

            try
            {
                var indexPattern = $"{_openSearchOptions.IndexPrefix}-requests-*";
                var fromDate = DateTime.UtcNow.AddHours(-hours);

                var response = await _openSearchClient.SearchAsync<RequestLogEntry>(s => s
                    .Index(indexPattern)
                    .Size(0)
                    .Query(q => q
                        .DateRange(dr => dr
                            .Field(f => f.TimestampUtc)
                            .GreaterThanOrEquals(fromDate)
                        )
                    )
                    .Aggregations(a => a
                        .Terms("by_path", t => t
                            .Field(f => f.Path.Suffix("keyword"))
                            .Size(5)
                            .Aggregations(aa => aa
                                .Terms("by_method", tm => tm
                                    .Field(f => f.Method.Suffix("keyword"))
                                    .Size(10)
                                )
                                .Terms("by_status_code", ts => ts
                                    .Field(f => f.StatusCode)
                                    .Size(10)
                                )
                                .Average("avg_duration", ad => ad
                                    .Field(f => f.DurationMs)
                                )
                                .Max("max_duration", md => md
                                    .Field(f => f.DurationMs)
                                )
                            )
                        )
                    )
                );

                if (!response.IsValid)
                {
                    return StatusCode(500, new { error = "Failed to query OpenSearch", message = response.DebugInformation });
                }

                var stats = response.Aggregations.Terms("by_path").Buckets.Select(bucket => new
                {
                    path = bucket.Key,
                    totalRequests = bucket.DocCount,
                    methods = bucket.Terms("by_method").Buckets.Select(mb => new
                    {
                        method = mb.Key,
                        count = mb.DocCount
                    }).ToList(),
                    statusCodes = bucket.Terms("by_status_code").Buckets.Select(sb => new
                    {
                        statusCode = int.Parse(sb.Key),
                        count = sb.DocCount
                    }).ToList(),
                    avgDurationMs = bucket.Average("avg_duration").Value,
                    maxDurationMs = bucket.Max("max_duration").Value
                }).OrderByDescending(s => s.totalRequests).ToList();

                return Ok(new
                {
                    enabled = true,
                    hours = hours,
                    timestamp = DateTime.UtcNow,
                    data = stats
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve request stats", message = ex.Message });
            }
        }

        /// <summary>
        /// Get paginated list of requests
        /// </summary>
        [HttpGet("requests")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRequests(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] int hours = 24)
        {
            if (!_openSearchOptions.Enabled)
            {
                return Ok(new { enabled = false, data = new object[] { }, total = 0, page = 1, pageSize = 20 });
            }

            try
            {
                var indexPattern = $"{_openSearchOptions.IndexPrefix}-requests-*";
                var fromDate = DateTime.UtcNow.AddHours(-hours);
                var from = (page - 1) * pageSize;

                var response = await _openSearchClient.SearchAsync<RequestLogEntry>(s => s
                    .Index(indexPattern)
                    .From(from)
                    .Size(pageSize)
                    .Query(q => q
                        .DateRange(dr => dr
                            .Field(f => f.TimestampUtc)
                            .GreaterThanOrEquals(fromDate)
                        )
                    )
                    .Sort(so => so
                        .Descending(f => f.TimestampUtc)
                    )
                );

                if (!response.IsValid)
                {
                    return StatusCode(500, new { error = "Failed to query OpenSearch", message = response.DebugInformation });
                }

                return Ok(new
                {
                    enabled = true,
                    data = response.Documents,
                    total = response.Total,
                    page = page,
                    pageSize = pageSize,
                    totalPages = (int)Math.Ceiling((double)response.Total / pageSize)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve requests", message = ex.Message });
            }
        }
    }
}
