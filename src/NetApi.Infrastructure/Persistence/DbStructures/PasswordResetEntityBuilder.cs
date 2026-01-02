using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class PasswordResetEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<PasswordResetEntity> builder)
    {
        builder.Property(pr => pr.Id).HasConversion(
            v => v!.ToGuid(),
            v => PasswordResetId.FromGuid(v)
        ).ValueGeneratedOnAdd();
        builder.Property(pr => pr.UserId).HasConversion(
            v => v.ToGuid(),
            v => UserId.FromGuid(v)
        ).IsRequired();

        builder.Property(pr => pr.Token).IsRequired().HasMaxLength(255);
        builder.Property(pr => pr.ExpiresAt).IsRequired();
        builder.Property(pr => pr.CreatedAt).IsRequired();

        builder
            .HasOne<UserEntity>()
            .WithMany()
            .HasForeignKey(pr => pr.UserId)
            .HasPrincipalKey(u => u.Id);

        builder.Ignore(pr => pr.IsUsed);
        builder.HasKey(pr => pr.Id);
        builder.HasIndex(pr => pr.Token).IsUnique();
    }
}
