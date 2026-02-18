namespace StargateAPI.Logging
{
    public class RequestLogEntry
    {
        public string RequestId { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string QueryString { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public long DurationMs { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string? UserAgent { get; set; }
        public string? RemoteIp { get; set; }
        public string? ExceptionId { get; set; }
    }
}
