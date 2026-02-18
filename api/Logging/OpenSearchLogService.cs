using OpenSearch.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace StargateAPI.Logging
{
    public class OpenSearchLogService : ILogService
    {
        private readonly IOpenSearchClient _client;
        private readonly OpenSearchOptions _options;

        public OpenSearchLogService(IOpenSearchClient client, OpenSearchOptions options)
        {
            _client = client;
            _options = options;
        }

        public async Task LogRequestAsync(RequestLogEntry entry, CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                return;
            }

            var indexName = BuildIndexName("requests", entry.TimestampUtc);

            try
            {
                await _client.IndexAsync(entry, d => d.Index(indexName), cancellationToken);
            }
            catch
            {
                // Avoid impacting the request pipeline if logging fails.
            }
        }

        public async Task LogExceptionAsync(ExceptionLogEntry entry, CancellationToken cancellationToken)
        {
            if (!_options.Enabled)
            {
                return;
            }

            var indexName = BuildIndexName("exceptions", entry.TimestampUtc);

            try
            {
                await _client.IndexAsync(entry, d => d.Index(indexName), cancellationToken);
            }
            catch
            {
                // Avoid impacting the request pipeline if logging fails.
            }
        }

        private string BuildIndexName(string suffix, DateTime timestampUtc)
        {
            return $"{_options.IndexPrefix}-{suffix}-{timestampUtc:yyyy.MM.dd}";
        }
    }
}
