using CubArt.Domain.Entities;
using CubArt.Infrastructure.Common;

namespace CubArt.Infrastructure.Interfaces
{
    public interface IUserRepository : IRepository<User, Guid>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
        Task<User?> GetByIdWithRolesAsync(Guid id);
        Task<User?> GetByUsernameWithRolesAsync(string username);
    }

}
