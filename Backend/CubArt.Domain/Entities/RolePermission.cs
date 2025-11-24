using CubArt.Domain.Common;
using CubArt.Domain.Enums;

namespace CubArt.Domain.Entities
{
    public class RolePermission : Entity<int>
    {
        public RolePermission(int roleId, PermissionEnum permission)
        {
            RoleId = roleId;
            Permission = permission;
        }

        public int RoleId { get; private set; }
        public PermissionEnum Permission { get; private set; }

        // Навигационные свойства
        public virtual Role Role { get; set; }
    }
}
