using Domain.Entities;
using Domain.Entities.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data
{
    public class DbContextTask : DbContext
    {
        public DbContextTask(DbContextOptions<DbContextTask> options) : base(options) { }

        // DbSet properties for entities
        public DbSet<User> Users { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserRoleUser> UserRoleUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; } // New DbSet for Payments

        // Fluent API configurations
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships and other properties

            // Many-to-many relationship between User and Role
            modelBuilder.Entity<User>()
                .HasMany(u => u.Roles)
                .WithMany(ur => ur.Users)
                .UsingEntity<UserRoleUser>(
                    j => j.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),
                    j => j.HasOne(u => u.User).WithMany().HasForeignKey(u => u.UserId),
                    j => j.HasKey(ur => new { ur.UserId, ur.RoleId })
                );

            // One-to-many relationship between RefreshToken and User
            modelBuilder.Entity<RefreshToken>()
                .HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // One-to-many relationship between Product and Category
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure precision for Price in Product
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasColumnType("decimal(18,2)");

            // Configure precision for TotalAmount in Order
            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");

            // Configure precision for Price in OrderItem
            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasColumnType("decimal(18,2)");

            // Configure the relationship between Order and Payment
            modelBuilder.Entity<Payment>()
                .HasKey(p => p.PaymentId);  // Primary key for Payment

            // Configure the relationship between Payment and Order (One-to-many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)  // An order can have many payments
                .HasForeignKey(p => p.OrderId)  // Foreign key to Order
                .OnDelete(DeleteBehavior.Cascade); // If the order is deleted, delete the payment as well

            // Configure Payment properties
            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentIntentId)
                .HasMaxLength(255)
                .IsRequired();

            modelBuilder.Entity<Payment>()
                .Property(p => p.Status)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Payment>()
                .Property(p => p.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Configure OrderItem
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Order properties
            modelBuilder.Entity<Order>()
                .Property(o => o.OrderDate)
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}
