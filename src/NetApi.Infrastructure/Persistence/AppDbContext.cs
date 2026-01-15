using Microsoft.EntityFrameworkCore;
using NetApi.Domain.Media.Entities;
using NetApi.Domain.Roles.Entities;
using NetApi.Domain.Users.Entities;
using NetApi.Infrastructure.Persistence.DbStructures;

namespace NetApi.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public required DbSet<LoginAttemptEntity> LoginAttempts { get; set; }
    public required DbSet<MediaEntity> Media { get; set; }
    public required DbSet<PasswordResetEntity> PasswordResets { get; set; }
    public required DbSet<RoleEntity> Roles { get; set; }
    public required DbSet<RolePermissionEntity> RolePermissions { get; set; }
    public required DbSet<UserEntity> Users { get; set; }
    public required DbSet<UserRoleEntity> UserRoles { get; set; }
    public required DbSet<UserSettingEntity> UserSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<LoginAttemptEntity>(LoginAttemptEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<MediaEntity>(MediaEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<RoleEntity>(RoleEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<RolePermissionEntity>(RolePermissionEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<UserEntity>(UserEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<UserRoleEntity>(UserRoleEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<PasswordResetEntity>(PasswordResetEntityBuilder.ConstructBuilder);
        modelBuilder.Entity<UserSettingEntity>(UserSettingEntityBuilder.ConstructBuilder);
    }
}
