namespace CubArt.Domain.Exceptions
{
    public class NotFoundException : DomainException
    {
        public string EntityName { get; }
        public object EntityId { get; }

        public NotFoundException(string entityName, object entityId)
            : base($"{entityName} с ID {entityId} не найден")
        {
            EntityName = entityName;
            EntityId = entityId;
        }

        public NotFoundException(string message) : base(message)
        {
        }

        public NotFoundException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

}
