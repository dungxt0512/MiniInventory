using Microsoft.EntityFrameworkCore;
using MiniInventory.Model.Entities;

namespace MiniInventory.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        protected AppDbContext()
        {
        }
        public DbSet<User> users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<InventoryTransaction> InventoryTransactions { get; set; }
        public DbSet<InventoryTransactionDetail> InventoryTransactionDetails { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
           modelBuilder.Entity<User>().HasData(
                new User { Id = 1, UserName = "admin", PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456"), Role = "Admin" }
            );
           modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(c => c.CategoryId);
           modelBuilder.Entity<InventoryTransactionDetail>()
                .HasOne(d => d.Transaction)
                .WithMany(c => c.Details)
                .HasForeignKey(c => c.TransactionId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<InventoryTransactionDetail>()
                .HasOne(p => p.Product)
                .WithMany()
                .HasForeignKey(c => c.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Product>()
                .Property(p => p.CostPrice)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<Product>()
                .Property(p => p.SalePrice)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<InventoryTransactionDetail>()
                .Property(p => p.UnitPrice)
                .HasColumnType("decimal(18,2)");
        }
    }
}
