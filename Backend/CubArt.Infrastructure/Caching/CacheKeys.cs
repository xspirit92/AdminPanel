using CubArt.Domain.Enums;

namespace CubArt.Infrastructure.Caching
{
    public static class CacheKeys
    {
        // Продукция
        public static string Product(int id) => $"product:{id}";
        public static string ProductsList(string filter) => $"products:list:{filter}";

        // Справочники
        public static string Suppliers => "suppliers:all";
        public static string Facilities => "facilities:all";
    }

}
