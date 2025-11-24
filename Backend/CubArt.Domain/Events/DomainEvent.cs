using MediatR;

namespace CubArt.Domain.Events
{
    public interface IDomainEvent : INotification
    {
        DateTime OccurredOn { get; }
    }

    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredOn { get; protected set; } = DateTime.UtcNow;
        public string EventType { get; protected set; }

        protected DomainEvent()
        {
            EventType = GetType().Name;
        }
    }
}
