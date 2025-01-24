using Microsoft.EntityFrameworkCore;
using CornerStore.Models;
public class CornerStoreDbContext : DbContext
{
    public DbSet<Cashier> Cashiers { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    public CornerStoreDbContext(DbContextOptions<CornerStoreDbContext> context) : base(context)
    {

    }

    //allows us to configure the schema when migrating as well as seed data
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cashier>().HasData(new Cashier[]
        {
            new Cashier {Id = 1, FirstName = "Mark", LastName = "Denmark"},
            new Cashier {Id = 2, FirstName = "John", LastName = "Simpson"},
            new Cashier {Id = 3, FirstName = "Sturgill", LastName = "McBride"},
            new Cashier {Id = 4, FirstName = "Beth", LastName = "Burbank"},
            new Cashier {Id = 5, FirstName = "Sarah", LastName = "Elisabeth"}
        });
        modelBuilder.Entity<Category>().HasData(new Category[]
        {
            new Category {Id = 1, CategoryName = "Food"},
            new Category {Id = 2, CategoryName = "Drink"},
            new Category {Id = 3, CategoryName = "Lotto"},
            new Category {Id = 4, CategoryName = "Gas"}
        });
        modelBuilder.Entity<Product>().HasData(new Product[]
        {
            new Product {Id = 1, ProductName = "Cool Ranch", Price = 14.99M, Brand = "Doritos", CategoryId = 1},
            new Product {Id = 2, ProductName = "Blue", Price = 4.89M, Brand = "Gatorade", CategoryId = 2},
            new Product {Id = 3, ProductName = "Powerball", Price = 2.00M, Brand = "Government", CategoryId = 3},
            new Product {Id = 4, ProductName = "Gas", Price = 2.89M, Brand = "Gas", CategoryId = 4}
        });
        modelBuilder.Entity<Order>().HasData(new Order[]
        {
            new Order {Id = 1, CashierId = 1, PaidOnDate = new DateTime(2024, 1, 12)},
            new Order {Id = 2, CashierId = 2, PaidOnDate = new DateTime(2024, 9, 3)},
            new Order {Id = 3, CashierId = 3, PaidOnDate = null},
            new Order {Id = 4, CashierId = 4, PaidOnDate = null},
            new Order {Id = 5, CashierId = 5, PaidOnDate = new DateTime(2025, 1, 1)}
        });
        modelBuilder.Entity<OrderProduct>().HasData(new OrderProduct[]
        {
            new OrderProduct {Id = 1, ProductId = 4, OrderId = 1, Quantity = 10},
            new OrderProduct {Id = 2 , ProductId = 2, OrderId = 2, Quantity = 1},
            new OrderProduct {Id = 3 , ProductId = 1, OrderId = 2, Quantity = 1},
            new OrderProduct {Id = 4 , ProductId = 3, OrderId = 3, Quantity = 2},
            new OrderProduct {Id = 5 , ProductId = 2, OrderId = 3, Quantity = 1},
            new OrderProduct {Id = 6 , ProductId = 1, OrderId = 4, Quantity = 2},
            new OrderProduct {Id = 7 , ProductId = 2, OrderId = 4, Quantity = 5},
            new OrderProduct {Id = 8 , ProductId = 4, OrderId = 4, Quantity = 5},
            new OrderProduct {Id = 9 , ProductId = 3, OrderId = 5, Quantity = 1}
        });
    }
}