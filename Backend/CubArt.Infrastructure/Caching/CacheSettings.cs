namespace CubArt.Infrastructure.Caching
{
    public static class CacheSettings
    {
        // Продукция
        public static readonly TimeSpan ProductDetails = TimeSpan.FromMinutes(30);
        public static readonly TimeSpan ProductList = TimeSpan.FromMinutes(15);

        // Общие настройки
        public static readonly TimeSpan Default = TimeSpan.FromMinutes(20);
    }
}
