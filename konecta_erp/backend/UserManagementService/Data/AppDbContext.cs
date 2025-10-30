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

                entity.Property(user => user.Role)
                    .HasMaxLength(64)
                    .HasDefaultValue("Employee");

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

                entity.HasIndex(user => new { user.Role, user.IsDeleted });
                entity.HasIndex(user => new { user.Department, user.IsDeleted });
            });
        }
    }
}
