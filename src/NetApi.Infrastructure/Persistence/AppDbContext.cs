using Microsoft.EntityFrameworkCore;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users.Entities;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<PasswordResetEntity> PasswordResets { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<UserSettingEntity> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<UserEntity>(builder => {
            builder.Property(u => u.Id).HasConversion(
                v => v.ToGuid(),
                v => UserId.FromGuid(v)
            );
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => EmailAddress.FromString(v)
                );
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
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasIndex(u => u.Username).IsUnique();
        });

        modelBuilder.Entity<RoleEntity>(builder => {
            builder.Property(r => r.Id)
                .HasConversion(
                    v => v!.Value,
                    v => RoleId.Create(v)
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
                    l => l.HasOne<UserEntity>().WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(UserEntity.Id)),
                    r => r.HasOne<RoleEntity>().WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(RoleEntity.Id)),
                    j => j.HasKey("UserId", "RoleId"));
        });

        modelBuilder.Entity<UserRoleEntity>(builder => {
            builder.Property(ur => ur.UserId).HasConversion(
                v => v.ToGuid(),
                v => UserId.FromGuid(v)
            );
            builder.Property(ur => ur.RoleId).HasConversion(
                v => v.Value,
                v => RoleId.Create(v)
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

        });

        modelBuilder.Entity<PasswordResetEntity>(builder => {
            builder.Property(pr => pr.Id).HasConversion(
                v => v.ToGuid(),
                v => PasswordResetId.FromGuid(v)
            ).ValueGeneratedOnAdd();
            builder.Property(pr => pr.UserId).HasConversion(
                v => v.ToGuid(),
                v => UserId.FromGuid(v)
            );
            builder.Property(pr => pr.Token).IsRequired().HasMaxLength(255);
            builder.Property(pr => pr.ExpiresAt).IsRequired();
            builder.Property(pr => pr.CreatedAt).IsRequired();

            builder.HasKey(pr => pr.Id);
            builder.HasIndex(pr => pr.Token).IsUnique();

            builder.Ignore(pr => pr.IsUsed);

            builder
                .HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(pr => pr.UserId)
                .HasPrincipalKey(u => u.Id);
        });

        modelBuilder.Entity<UserSettingEntity>(builder => {
            // Composite Key: UserId + Key
            builder.HasKey(us => new { us.UserId, us.Key });

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

            builder.HasKey(us => new { us.UserId, us.Key });

            builder
                .HasOne(us => us.User)
                .WithMany(u => u.UserSettings)
                .HasForeignKey(us => us.UserId)
                .HasPrincipalKey(u => u.Id);

            builder.HasIndex(us => new { us.UserId, us.Key }).IsUnique();
        });

    }
}
