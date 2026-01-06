using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class RolePermissionEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<RolePermissionEntity> builder)
    {
        builder.Property(x => x.Id).HasConversion(
            x => x!.ToGuid(),
            x => RolePermissionId.FromGuid(x)
        );
        builder.Property(x => x.RoleId).HasConversion(
            v => v.ToShort(),
            v => RoleId.FromShort(v)
        );
        builder.Property(x => x.Feature).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Action).IsRequired();
        builder.Property(x => x.IsAllowed).IsRequired();

        builder.HasKey(x => x.Id);

        builder
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .HasPrincipalKey(r => r.Id);
    }
}

