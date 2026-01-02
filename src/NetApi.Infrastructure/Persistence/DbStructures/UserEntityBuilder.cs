using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class UserEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<UserEntity> builder)
    {
        builder.Property(u => u.Id)
            .HasConversion(
                v => v!.ToGuid(),
                v => UserId.FromGuid(v)
            )
            .ValueGeneratedOnAdd();
        builder.Property(u => u.EmailAddress)
            .IsRequired()
            .HasMaxLength(100)
            .HasConversion(
                v => v.ToString(),
                v => EmailAddress.FromString(v)
            );

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
        builder.Property(u => u.PasswordHash).IsRequired();
        builder.Property(u => u.RefreshToken).IsRequired(false).HasMaxLength(255);
        builder.Property(u => u.RefreshTokenExpiryTime).IsRequired(false);
        builder.Property(u => u.CreatedAt).IsRequired();
        builder.Property(u => u.CreatedBy).IsRequired().HasMaxLength(50);
        builder.Property(u => u.UpdatedAt).IsRequired();
        builder.Property(u => u.UpdatedBy).IsRequired().HasMaxLength(50);
        builder.Property(u => u.DeletedAt).IsRequired(false);
        builder.Property(u => u.DeletedBy).IsRequired(false).HasMaxLength(50);

        builder.HasKey(u => u.Id);
        builder.HasIndex(u => u.EmailAddress).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();
    }

    private class UserIdGenerator : ValueGenerator<UserId>
    {
        public override bool GeneratesTemporaryValues => false;

        public override UserId Next(EntityEntry entry)
            => UserId.New();
    }
}
