namespace CubArt.Domain.Enums
{
    public enum PurchaseStatusEnum
    {
        // Ожидает оплаты
        Pending = 1,
        // Оплачена
        Paid = 2,
        // Завершена
        Completed = 3,
        // Отменена
        Cancelled = 4
    }
}
