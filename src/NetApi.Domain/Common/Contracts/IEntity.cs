namespace NetApi.Domain.Common.Contracts;

public interface IEntity<TKey> where TKey : notnull
{
    TKey Id { get; set; }
}
