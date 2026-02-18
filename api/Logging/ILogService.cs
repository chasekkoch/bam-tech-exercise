using System.Threading;
using System.Threading.Tasks;

namespace StargateAPI.Logging
{
    public interface ILogService
    {
        Task LogRequestAsync(RequestLogEntry entry, CancellationToken cancellationToken);
        Task LogExceptionAsync(ExceptionLogEntry entry, CancellationToken cancellationToken);
    }
}
