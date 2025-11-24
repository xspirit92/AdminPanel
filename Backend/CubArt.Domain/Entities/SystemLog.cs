using CubArt.Domain.Common;

namespace CubArt.Domain.Entities
{
    public class SystemLog : Entity<Guid>, IHasCreatedDate
    {
        public SystemLog(
            string level, 
            string message, 
            string? userId = null,
            string? ipAddress = null,
            string? userAgent = null,
            string? requestPath = null,
            string? requestMethod = null,
            string? exceptionType = null,
            string? source = null,
            string? action = null,
            string? entityType = null,
            string? entityId = null,
            string? additionalData = null
            )
        {
            Level = level;
            Message = message;
            UserId = userId;
            IpAddress = ipAddress;
            UserAgent = userAgent;
            RequestPath = requestPath;
            RequestMethod = requestMethod;
            ExceptionType = exceptionType;
            Source = source;
            Action = action;
            EntityType = entityType;
            EntityId = entityId;
            AdditionalData = additionalData;
        }

        public string Level { get; private set; }
        public string Message { get; private set; }
        public string? UserId { get; private set; }
        public string? IpAddress { get; private set; }
        public string? UserAgent { get; private set; }
        public string? RequestPath { get; private set; }
        public string? RequestMethod { get; private set; }
        public string? ExceptionType { get; private set; }
        public string? Source { get; private set; }
        public string? Action { get; private set; }
        public string? EntityType { get; private set; }
        public string? EntityId { get; private set; }
        public string? AdditionalData { get; private set; }
        public DateTime DateCreated { get; set; }
    }

}
