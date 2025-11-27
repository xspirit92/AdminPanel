namespace CubArt.Infrastructure.Caching
{
    public static class CacheKeys
    {
        private static string _listKey = "list";
        private static string _pagedListKey = "pagedlist";

        // Продукция
        public static string ProductPattern => $"{nameof(Product)}";
        public static string Product(int id) => $"{nameof(Product)}:{id}";
        public static string ProductList(string filter) => $"{nameof(Product)}:{_listKey}:{filter}";
        public static string ProductsPagedList(string filter) => $"{nameof(Product)}:{_pagedListKey}:{filter}";

        // Справочники
        public static string SupplierList => $"{nameof(SupplierList)}";
        public static string FacilityList => $"{nameof(FacilityList)}";

        // Закупки
        public static string PurchasePattern => $"{nameof(Purchase)}";
        public static string Purchase(Guid id) => $"{nameof(Purchase)}:{id}";
        public static string PurchaseList(string filter) => $"{nameof(Purchase)}:{_listKey}:{filter}";
        public static string PurchasePagedList(string filter) => $"{nameof(Purchase)}:{_pagedListKey}:{filter}";

        // Оплаты
        public static string PaymentPattern => $"{nameof(Payment)}";
        public static string Payment(Guid id) => $"{nameof(Payment)}:{id}";
        public static string PaymentPagedList(string filter) => $"{nameof(Payment)}:{_pagedListKey}:{filter}";

        // Поставки
        public static string SupplyPattern => $"{nameof(Supply)}";
        public static string Supply(Guid id) => $"{nameof(Supply)}:{id}";
        public static string SupplyPagedList(string filter) => $"{nameof(Supply)}: {_pagedListKey} :{filter}";

        // Производство
        public static string ProductionPattern => $"{nameof(Production)}";
        public static string Production(Guid id) => $"{nameof(Production)}:{id}";
        public static string ProductionPagedList(string filter) => $"{nameof(Production)}: {_pagedListKey} :{filter}";
    }

}
