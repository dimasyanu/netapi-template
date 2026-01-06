using NetApi.Domain.Common.Contracts;

namespace NetApi.Domain.Common.Extensions;

public static class TimestampExtension
{
    public static ITimestamp SetCreated(this ITimestamp entity, string creator)
    {
        entity.CreatedAt = DateTime.Now;
        entity.CreatedBy = creator;
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = creator;

        return entity;
    }

    public static TEntity SetCreated<TEntity>(this TEntity entity, string creator) where TEntity: ITimestamp
    {
        entity.CreatedAt = DateTime.Now;
        entity.CreatedBy = creator;
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = creator;

        return entity;
    }

    public static ITimestamp SetUpdated(this ITimestamp entity, string author)
    {
        entity.UpdatedAt = DateTime.Now;
        entity.UpdatedBy = author;

        return entity;
    }
}

