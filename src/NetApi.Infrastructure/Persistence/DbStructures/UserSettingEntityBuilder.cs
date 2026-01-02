using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class UserSettingEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<UserSettingEntity> builder)
    {
        builder.Property(us => us.UserId).HasConversion(
            v => v.ToGuid(),
            v => UserId.FromGuid(v)
        ).IsRequired();

        builder.Property(us => us.Key).IsRequired().HasMaxLength(100);
        builder.Property(us => us.Value).IsRequired().HasMaxLength(500);
        builder.Property(us => us.CreatedAt).IsRequired();
        builder.Property(us => us.CreatedBy).IsRequired().HasMaxLength(50);
        builder.Property(us => us.UpdatedAt).IsRequired();
        builder.Property(us => us.UpdatedBy).IsRequired().HasMaxLength(50);

        builder
            .HasOne(us => us.User)
            .WithMany(u => u.UserSettings)
            .HasForeignKey(us => us.UserId)
            .HasPrincipalKey(u => u.Id);

        // Composite Key: UserId + Key
        builder.HasKey(us => new { us.UserId, us.Key });
        builder.HasIndex(us => new { us.UserId, us.Key }).IsUnique();
    }
}
