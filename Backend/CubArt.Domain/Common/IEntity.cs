using CubArt.Domain.Events;

namespace CubArt.Domain.Common
{
    public interface IEntity
    {
        IReadOnlyCollection<DomainEvent> DomainEvents { get; }
        void AddDomainEvent(DomainEvent eventItem);
        void RemoveDomainEvent(DomainEvent eventItem);
        void ClearDomainEvents();
    }

}
