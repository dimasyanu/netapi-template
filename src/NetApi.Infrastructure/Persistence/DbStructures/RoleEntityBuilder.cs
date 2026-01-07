using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.Entities;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class RoleEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.Property(r => r.Id)
            .HasConversion(
                v => v!.ToShort(),
                v => RoleId.FromShort(v)
            )
            .ValueGeneratedOnAdd();

        builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
        builder.Property(r => r.Description).IsRequired(false).HasMaxLength(255);
        builder.Property(r => r.CreatedAt).IsRequired();
        builder.Property(r => r.CreatedBy).IsRequired().HasMaxLength(50);
        builder.Property(r => r.UpdatedAt).IsRequired();
        builder.Property(r => r.UpdatedBy).IsRequired().HasMaxLength(50);
        builder.Property(r => r.DeletedAt).IsRequired(false);
        builder.Property(r => r.DeletedBy).IsRequired(false).HasMaxLength(50);

        builder.HasKey(r => r.Id);
        builder.HasIndex(r => r.Name).IsUnique();

        builder
            .HasMany(r => r.Users)
            .WithMany(u => u.Roles)
            .UsingEntity<UserRoleEntity>(
                l => l.HasOne<UserEntity>().WithMany().HasForeignKey(nameof(UserRoleEntity.UserId)).HasPrincipalKey(nameof(UserEntity.Id)),
                r => r.HasOne<RoleEntity>().WithMany().HasForeignKey(nameof(UserRoleEntity.RoleId)).HasPrincipalKey(nameof(RoleEntity.Id)),
                j => j.HasKey(nameof(UserRoleEntity.UserId), nameof(UserRoleEntity.RoleId)));

        builder
            .HasMany(r => r.Permissions)
            .WithOne(p => p.Role)
            .HasForeignKey(p => p.RoleId)
            .HasPrincipalKey(r => r.Id);
    }
}
