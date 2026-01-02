using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class UserRoleEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<UserRoleEntity> builder)
    {
        builder.Property(ur => ur.UserId).HasConversion(
            v => v.ToGuid(),
            v => UserId.FromGuid(v)
        );
        builder.Property(ur => ur.RoleId).HasConversion(
            v => v.ToShort(),
            v => RoleId.FromShort(v)
        );
        builder.Property(ur => ur.AssignedAt).IsRequired();

        builder.HasKey(ur => new { ur.UserId, ur.RoleId });

        builder
            .HasOne(ur => ur.User)
            .WithMany()
            .HasForeignKey(ur => ur.UserId)
            .HasPrincipalKey(u => u.Id);

        builder
            .HasOne(ur => ur.Role)
            .WithMany()
            .HasForeignKey(ur => ur.RoleId)
            .HasPrincipalKey(r => r.Id);
    }
}
