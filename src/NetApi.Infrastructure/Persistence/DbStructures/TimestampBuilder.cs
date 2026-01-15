using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Common.Contracts;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class TimestampBuilder
{
    public static void ConstructBuilder<TEntity>(EntityTypeBuilder<TEntity> builder) where TEntity : class, ITimestamp
    {
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
    }
}
