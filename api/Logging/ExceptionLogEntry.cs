namespace StargateAPI.Logging
{
    public class ExceptionLogEntry
    {
        public string ExceptionId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string Method { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string QueryString { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ExceptionType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string? UserAgent { get; set; }
        public string? RemoteIp { get; set; }
    }
}
