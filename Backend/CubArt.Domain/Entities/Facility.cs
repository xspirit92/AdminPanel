using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class Facility : Entity<int>
    {
        public string Name { get; set; }
        public string? Address { get; set; }
    }
}
