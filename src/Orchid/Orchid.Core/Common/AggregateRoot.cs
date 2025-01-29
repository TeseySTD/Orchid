namespace Orchid.Core.Common;

public abstract class AggregateRoot<TId>(TId id) : IEntity<TId>
{
    public TId Id { get; set; } = id;
}