using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class Supplier : Entity<int>
    {
        public string Name { get; set; }
    }
}
