namespace NetApi.Domain.Common.Contracts;

public interface IEntity
{
}

public interface IEntity<TKey> : IEntity where TKey : notnull
{
    TKey Id { get; set; }
}
