using CloudRetailWebApp.Models;
using Microsoft.EntityFrameworkCore; 
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<CartItem>()
                .HasOne(c => c.User) /
                .WithMany(u => u.CartItems) 
                .HasForeignKey(c => c.UserId) 
                .OnDelete(DeleteBehavior.ClientSetNull); 

           
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User) 
                .WithMany(u => u.Orders) 
                .HasForeignKey(o => o.UserId) 
                .OnDelete(DeleteBehavior.ClientSetNull); 
          
        }
    }
}