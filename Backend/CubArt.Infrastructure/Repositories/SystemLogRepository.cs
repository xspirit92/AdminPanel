using CubArt.Domain.Entities;
using CubArt.Infrastructure.Data;
using CubArt.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CubArt.Infrastructure.Repositories
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly LogDbContext _context;

        public SystemLogRepository(LogDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(SystemLog log)
        {
            await _context.SystemLogs.AddAsync(log);
            await _context.SaveChangesAsync();
        }

        public async Task AddRangeAsync(IEnumerable<SystemLog> logs)
        {
            await _context.SystemLogs.AddRangeAsync(logs);
            await _context.SaveChangesAsync();
        }

        public async Task CleanOldLogsAsync(DateTime olderThan, CancellationToken cancellationToken)
        {
            var logsToDelete = await _context.SystemLogs
                .Where(x => x.DateCreated < olderThan)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            _context.SystemLogs.RemoveRange(logsToDelete);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

}
