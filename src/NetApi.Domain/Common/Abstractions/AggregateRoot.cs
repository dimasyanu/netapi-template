namespace NetApi.Domain.Common.Abstractions;

public class AggregateRoot<TId>(TId id) : Entity<TId>(id) where TId : notnull
{
}
