using Microsoft.EntityFrameworkCore;
using UserManagementService.Models;

namespace UserManagementService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Permission> Permissions => Set<Permission>();
        public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
        public DbSet<UserRole> UserRoles => Set<UserRole>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(user => user.Id);

                entity.Property(user => user.Id)
                    .HasMaxLength(64);

                entity.Property(user => user.Email)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(user => user.NormalizedEmail)
                    .IsRequired()
                    .HasMaxLength(256);

                entity.Property(user => user.FullName)
                    .IsRequired()
                    .HasMaxLength(128);

                entity.Property(user => user.FirstName)
                    .HasMaxLength(64);

                entity.Property(user => user.LastName)
                    .HasMaxLength(64);

                entity.Property(user => user.Department)
                    .HasMaxLength(128);

                entity.Property(user => user.JobTitle)
                    .HasMaxLength(128);

                entity.Property(user => user.Status)
                    .HasMaxLength(32)
                    .HasDefaultValue("Active");

                entity.Property(user => user.PhoneNumber)
                    .HasMaxLength(32);

                entity.Property(user => user.ManagerId)
                    .HasMaxLength(64);

                entity.Property(user => user.RowVersion)
                    .IsRowVersion()
                    .IsConcurrencyToken();

                entity.HasIndex(user => user.Email)
                    .IsUnique();

                entity.HasIndex(user => user.NormalizedEmail)
                    .IsUnique();
                entity.HasIndex(user => new { user.Department, user.IsDeleted });
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(role => role.Id);
                entity.Property(role => role.Name)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(role => role.Description)
                    .HasMaxLength(256);
                entity.HasIndex(role => role.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<Permission>(entity =>
            {
                entity.HasKey(permission => permission.Id);
                entity.Property(permission => permission.Name)
                    .IsRequired()
                    .HasMaxLength(128);
                entity.Property(permission => permission.Description)
                    .HasMaxLength(256);
                entity.Property(permission => permission.Category)
                    .HasMaxLength(64);
                entity.HasIndex(permission => permission.Name)
                    .IsUnique();
            });

            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });
                entity.HasOne(rp => rp.Role)
                    .WithMany(role => role.Permissions!)
                    .HasForeignKey(rp => rp.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(rp => rp.Permission)
                    .WithMany(permission => permission.RoleAssignments!)
                    .HasForeignKey(rp => rp.PermissionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<UserRole>(entity =>
            {
                entity.HasKey(ur => new { ur.UserId, ur.RoleId });
                entity.HasOne(ur => ur.User)
                    .WithMany(user => user.UserRoles!)
                    .HasForeignKey(ur => ur.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ur => ur.Role)
                    .WithMany(role => role.UserRoles!)
                    .HasForeignKey(ur => ur.RoleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
