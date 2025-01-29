namespace Orchid.Core.Common;

public abstract class Entity<TId>
{
    public TId Id { get; set; }
}