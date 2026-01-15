using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Media.ValueObjects;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence.DbStructures;

public class MediaEntityBuilder
{
    public static void ConstructBuilder(EntityTypeBuilder<MediaEntity> builder)
    {
        builder.Property(x => x.Id)
            .HasConversion(
                x => x!.ToGuid(),
                x => MediaId.FromGuid(x)
            )
            .ValueGeneratedOnAdd();
        builder.Property(pr => pr.UserId).HasConversion(
            x => x!.ToGuid(),
            x => UserId.FromGuid(x)
        ).IsRequired();

        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Format).HasMaxLength(50);
        builder.Property(x => x.SizeInKb).IsRequired();
        builder.Property(x => x.Path).HasMaxLength(512).IsRequired();
        builder.Property(x => x.MediaType)
            .HasConversion(
                x => x.ToByte(),
                x => MediaType.FromByte(x)
            )
            .IsRequired();

        TimestampBuilder.ConstructBuilder(builder);

        builder.HasOne(x => x.User)
            .WithMany(y => y.Media)
            .HasForeignKey(x => x.UserId)
            .HasPrincipalKey(y => y.Id);
    }
}
