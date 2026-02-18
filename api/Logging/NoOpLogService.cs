using System.Threading;
using System.Threading.Tasks;

namespace StargateAPI.Logging
{
    public class NoOpLogService : ILogService
    {
        public Task LogRequestAsync(RequestLogEntry entry, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task LogExceptionAsync(ExceptionLogEntry entry, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
