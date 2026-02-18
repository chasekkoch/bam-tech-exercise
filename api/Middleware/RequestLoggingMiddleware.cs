using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using StargateAPI.Controllers;
using StargateAPI.Logging;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace StargateAPI.Middleware
{
    public class RequestLoggingMiddleware : IMiddleware
    {
        private readonly ILogService _logService;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(ILogService logService, ILogger<RequestLoggingMiddleware> logger)
        {
            _logService = logService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var stopwatch = Stopwatch.StartNew();
            string? exceptionId = null;

            try
            {
                await next(context);
            }
            catch (BadHttpRequestException ex)
            {
                exceptionId = Guid.NewGuid().ToString("N");
                await LogExceptionAsync(context, ex, (int)HttpStatusCode.BadRequest, exceptionId);
                await WriteErrorResponseAsync(context, ex.Message, (int)HttpStatusCode.BadRequest, exceptionId);
            }
            catch (Exception ex)
            {
                exceptionId = Guid.NewGuid().ToString("N");
                await LogExceptionAsync(context, ex, (int)HttpStatusCode.InternalServerError, exceptionId);
                await WriteErrorResponseAsync(
                    context,
                    "An unexpected error occurred. Please report the exception id to support.",
                    (int)HttpStatusCode.InternalServerError,
                    exceptionId);
            }
            finally
            {
                stopwatch.Stop();
                await LogRequestAsync(context, stopwatch.ElapsedMilliseconds, exceptionId);
            }
        }

        private async Task LogRequestAsync(HttpContext context, long durationMs, string? exceptionId)
        {
            try
            {
                var entry = new RequestLogEntry
                {
                    RequestId = context.TraceIdentifier,
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value ?? string.Empty : string.Empty,
                    StatusCode = context.Response.StatusCode,
                    DurationMs = durationMs,
                    TimestampUtc = DateTime.UtcNow,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString(),
                    ExceptionId = exceptionId
                };

                await _logService.LogRequestAsync(entry, context.RequestAborted);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to log request.");
            }
        }

        private async Task LogExceptionAsync(HttpContext context, Exception ex, int statusCode, string exceptionId)
        {
            try
            {
                var entry = new ExceptionLogEntry
                {
                    ExceptionId = exceptionId,
                    RequestId = context.TraceIdentifier,
                    Method = context.Request.Method,
                    Path = context.Request.Path,
                    QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value ?? string.Empty : string.Empty,
                    StatusCode = statusCode,
                    ExceptionType = ex.GetType().Name,
                    Message = ex.Message,
                    StackTrace = ex.StackTrace,
                    TimestampUtc = DateTime.UtcNow,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    RemoteIp = context.Connection.RemoteIpAddress?.ToString()
                };

                await _logService.LogExceptionAsync(entry, context.RequestAborted);
            }
            catch (Exception logEx)
            {
                _logger.LogWarning(logEx, "Failed to log exception.");
            }
        }

        private static async Task WriteErrorResponseAsync(HttpContext context, string message, int statusCode, string exceptionId)
        {
            if (context.Response.HasStarted)
            {
                return;
            }

            context.Response.Clear();
            context.Response.StatusCode = statusCode;
            context.Response.ContentType = "application/json";

            var payload = new BaseResponse
            {
                Success = false,
                Message = message,
                ResponseCode = statusCode,
                ExceptionId = exceptionId
            };

            await JsonSerializer.SerializeAsync(context.Response.Body, payload);
        }
    }
}
