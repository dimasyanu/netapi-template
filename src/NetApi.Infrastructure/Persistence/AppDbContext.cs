using Microsoft.EntityFrameworkCore;
using NetApi.Domain.Roles;
using NetApi.Domain.Roles.ValueObjects;
using NetApi.Domain.Users;
using NetApi.Domain.Users.ValueObjects;

namespace NetApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        #region User Entity Configuration
        modelBuilder.Entity<User>(builder => {
            builder.Property(u => u.Id).HasConversion(
                v => v.ToGuid(),
                v => UserId.Create(v)
            );
            builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Username).IsRequired().HasMaxLength(50);
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100)
                .HasConversion(
                    v => v.ToString(),
                    v => EmailAddress.Create(v)
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
        #endregion

        #region Role Entity Configuration
        modelBuilder.Entity<Role>(builder => {
            builder.Property(r => r.Id)
                .HasConversion(
                    v => v.Value,
                    v => RoleId.Create(v)
                );
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
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoles",
                    l => l.HasOne<User>().WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(User.Id)),
                    r => r.HasOne<Role>().WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(Role.Id)),
                    j => j.HasKey("UserId", "RoleId"));
        });
        #endregion

    }
}
