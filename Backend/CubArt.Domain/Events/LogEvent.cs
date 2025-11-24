namespace CubArt.Domain.Events
{
    public abstract class LogEvent : DomainEvent
    {
        public string Level { get; protected set; }
        public string Message { get; protected set; }
        public string? UserId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
        public Dictionary<string, object>? AdditionalData { get; set; }
    }

    public class InformationLogEvent : LogEvent
    {
        public InformationLogEvent(string message, Dictionary<string, object>? additionalData = null)
        {
            Level = "Information";
            Message = message;
            AdditionalData = additionalData;
        }
    }

    public class WarningLogEvent : LogEvent
    {
        public WarningLogEvent(string message, Dictionary<string, object>? additionalData = null)
        {
            Level = "Warning";
            Message = message;
            AdditionalData = additionalData;
        }
    }

    public class ErrorLogEvent : LogEvent
    {
        public string? StackTrace { get; set; }
        public string? ExceptionType { get; set; }

        public ErrorLogEvent(string message, Exception? ex = null, Dictionary<string, object>? additionalData = null)
        {
            Level = "Error";
            Message = message;
            AdditionalData = additionalData;

            if (ex != null)
            {
                StackTrace = ex.StackTrace;
                ExceptionType = ex.GetType().Name;
            }
        }
    }

    // Бизнес-события логирования
    public class PurchaseCreatedLogEvent : InformationLogEvent
    {
        public Guid PurchaseId { get; }
        public int SupplierId { get; set; }
        public int ProductId { get; set; }
        public int FacilityId { get; set; }
        public decimal Amount { get; set; }
        public decimal Quantity { get; set; }

        public PurchaseCreatedLogEvent(Guid purchaseId, int supplierId, int productId, int facilityId, decimal amount, decimal quantity)
            : base($"Создана закупка #purchaseId={purchaseId} supplierId={supplierId}, productId={productId}, facilityId={facilityId}, amount={amount}, quantity={quantity}")
        {
            PurchaseId = purchaseId;
            SupplierId = supplierId;
            ProductId = productId;
            FacilityId = facilityId;
            Amount = amount;
            Quantity = quantity;
        }
    }
}
