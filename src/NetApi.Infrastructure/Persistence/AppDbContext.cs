using Microsoft.EntityFrameworkCore;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.Entities;
using NetApi.Infrastructure.Persistence.DbStructures;

namespace NetApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<LoginAttemptEntity> LoginAttempts { get; set; }
    public DbSet<PasswordResetEntity> PasswordResets { get; set; }
    public DbSet<RoleEntity> Roles { get; set; }
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserRoleEntity> UserRoles { get; set; }
    public DbSet<UserSettingEntity> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoginAttemptEntity>(LoginAttemptEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<UserEntity>(UserEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<RoleEntity>(RoleEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<UserRoleEntity>(UserRoleEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<PasswordResetEntity>(PasswordResetEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<UserSettingEntity>(UserSettingEntityBuilder.ConstructBuilder);
    }
}
