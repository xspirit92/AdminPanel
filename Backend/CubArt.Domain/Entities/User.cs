using CubArt.Domain.Common;
using CubArt.Domain.Enums;

namespace CubArt.Domain.Entities
{
    public class User : Entity<Guid>, IHasCreatedDate
    {
        public User(string username, string email, string passwordHash, string firstName, string lastName)
        {
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            IsActive = true;
            DateCreated = DateTime.UtcNow;
        }

        public string Username { get; private set; }
        public string Email { get; private set; }
        public string PasswordHash { get; private set; }
        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLogin { get; private set; }

        public void UpdateLastLogin()
        {
            LastLogin = DateTime.UtcNow;
        }

        public void Deactivate()
        {
            IsActive = false;
        }

        public void UpdateProfile(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
        }

        public void ChangePassword(string newPasswordHash)
        {
            PasswordHash = newPasswordHash;
        }

        public void AssignRole(int roleId)
        {
            if (!UserRoles.Any(ur => ur.RoleId == roleId))
            {
                UserRoles.Add(new UserRole(Id, roleId));
            }
        }

        public void RemoveRole(int roleId)
        {
            var userRole = UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole != null)
            {
                UserRoles.Remove(userRole);
            }
        }

        public bool HasPermission(PermissionEnum permission)
        {
            return UserRoles.Any(ur =>
                ur.Role.RolePermissions.Any(rp => rp.Permission == permission));
        }

        public IEnumerable<PermissionEnum> GetPermissions()
        {
            return UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct();
        }

        public IEnumerable<string> GetRoleNames()
        {
            return UserRoles.Select(ur => ur.Role.Name).Distinct();
        }

        // Навигационные свойства
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }

}
