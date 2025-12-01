namespace NetApi.Domain.Common.Contracts;

public interface IHasEntity<TEntity>
{
    TEntity ToEntity();
}
