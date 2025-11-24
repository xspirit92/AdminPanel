using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class UserRole : Entity<int>
    {
        public UserRole(Guid userId, int roleId)
        {
            UserId = userId;
            RoleId = roleId;
        }

        public Guid UserId { get; private set; }
        public int RoleId { get; private set; }

        // Навигационные свойства
        public virtual User User { get; set; }
        public virtual Role Role { get; set; }
    }

}
