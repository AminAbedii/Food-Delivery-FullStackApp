using Food_Delivery_BackEnd.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.SqlServer;

namespace Food_Delivery_BackEnd.Data.Context
{
    public class FoodDeliveryDbContext : DbContext
    {
        public FoodDeliveryDbContext(DbContextOptions<FoodDeliveryDbContext> options) : base(options)
        {
        }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Partner> Partners { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Store> Stores { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FoodDeliveryDbContext).Assembly);

            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Partner>().ToTable("Partners");
            modelBuilder.Entity<Customer>().ToTable("Customers");
            modelBuilder.Entity<Store>().ToTable("Stores");
            modelBuilder.Entity<Product>().ToTable("Products");
            modelBuilder.Entity<Order>().ToTable("Orders");
            modelBuilder.Entity<OrderItem>().ToTable("OrderItems");
            modelBuilder.Entity<RefreshToken>().ToTable("RefreshTokens");


            modelBuilder.Entity<Product>()
           .HasKey(p => p.Id);

            modelBuilder.Entity<OrderItem>()
                .HasKey(oi => oi.Id);

            // Configure the foreign key with no cascade delete  
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Product) // Assuming 'Product' is your navigation property in OrderItem  
                .WithMany(p => p.OrderItems) // Assuming 'OrderItems' is your navigation property in Product  
                .HasForeignKey(oi => oi.ProductId) // Foreign key  
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete to avoid cycles
        }

        //public class FoodDeliveryDbContextFactory : IDesignTimeDbContextFactory<FoodDeliveryDbContext>
        //{
        //    public FoodDeliveryDbContext CreateDbContext(string[] args)
        //    {
        //        var optionsBuilder = new DbContextOptionsBuilder<FoodDeliveryDbContext>();
        //        optionsBuilder.UseSqlServer("FoodDeliveryConnection");

        //        return new FoodDeliveryDbContext(optionsBuilder.Options);
        //    }
        //}
    }
}
