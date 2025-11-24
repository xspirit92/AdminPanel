using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class Role : Entity<int>
    {
        public Role(string name, string? description)
        {
            Name = name;
            Description = description;
        }

        public string Name { get; private set; }
        public string? Description { get; private set; }

        // Навигационные свойства
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }

}
