namespace StargateAPI.Logging
{
    public class OpenSearchOptions
    {
        public bool Enabled { get; set; } = true;
        public string Uri { get; set; } = "http://localhost:9200";
        public string IndexPrefix { get; set; } = "stargate";
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
