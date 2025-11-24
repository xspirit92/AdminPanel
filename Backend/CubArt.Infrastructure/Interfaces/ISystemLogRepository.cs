using CubArt.Domain.Entities;

namespace CubArt.Infrastructure.Interfaces
{
    public interface ISystemLogRepository
    {
        Task AddAsync(SystemLog log);

        Task AddRangeAsync(IEnumerable<SystemLog> logs);
        Task CleanOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken);
    }
}
