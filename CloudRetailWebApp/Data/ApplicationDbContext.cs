// Data/ApplicationDbContext.cs

using CloudRetailWebApp.Models;
using Microsoft.EntityFrameworkCore; // Ensure this using statement is present

namespace CloudRetailWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }

        // Configure relationships if needed (optional but recommended)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationship: CartItem -> User
            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User) // CartItem has one User
                .WithMany(u => u.CartItems) // User has many CartItems
                .HasForeignKey(c => c.UserId) // UserId is the foreign key in CartItem
                .OnDelete(DeleteBehavior.ClientSetNull); // Prevent orphaned CartItems if User is deleted

            // Configure relationship: Order -> User
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User) // Order has one User
                .WithMany(u => u.Orders) // User has many Orders
                .HasForeignKey(o => o.UserId) // UserId is the foreign key in Order
                .OnDelete(DeleteBehavior.ClientSetNull); // Prevent orphaned Orders if User is deleted

            // Configure other relationships if you add OrderItem later.
        }
    }
}