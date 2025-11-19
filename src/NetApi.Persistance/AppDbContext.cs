using Microsoft.EntityFrameworkCore;
using NetApi.Domain.Entities;

namespace NetApi.Domain.Util;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Roles)
            .WithMany(r => r.Users)
            .UsingEntity<Dictionary<string, object>>(
                "UserRoles",
                r => r.HasOne<Role>().WithMany().HasForeignKey("RoleId").HasPrincipalKey(nameof(Role.Id)),
                l => l.HasOne<User>().WithMany().HasForeignKey("UserId").HasPrincipalKey(nameof(User.Id)),
                j => j.HasKey("UserId", "RoleId"));

        base.OnModelCreating(modelBuilder);
    }
}
