using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class LoginAttemptEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<LoginAttemptEntity> builder)
    {
        builder.Property(x => x.Id).HasConversion(
            v => v!.ToGuid(),
            v => LoginAttemptId.FromGuid(v)
        );
        builder.Property(x => x.UserId).HasConversion(
            v => v.ToGuid(),
            v => UserId.FromGuid(v)
        ).IsRequired();

        builder.Property(x => x.Location).HasMaxLength(255);
        builder.Property(x => x.IpAddress).HasMaxLength(20);
        builder.Property(x => x.Success).IsRequired();
        builder.Property(x => x.AttemptDateTime).IsRequired().ValueGeneratedOnAdd();

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .HasPrincipalKey(x => x.Id);

        builder.HasKey(x => x.Id);
    }
}
