using CornerStore.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using CornerStore.Models.DTOs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// allows passing datetimes without time zone data 
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

// allows our api endpoints to access the database through Entity Framework Core and provides dummy value for testing
builder.Services.AddNpgsql<CornerStoreDbContext>(builder.Configuration["CornerStoreDbConnectionString"] ?? "testing");

// Set the JSON serializer options
builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//endpoints go here

//-----------> Cashiers <------------------

app.MapGet("/cashiers/{id}", (int id, CornerStoreDbContext db) => 
{
    return db.Cashiers
    .Include(c => c.Orders)
        .ThenInclude(o => o.Cashier)
    .Include(c => c.Orders)
        .ThenInclude(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
    .Select(c => new CashierDTO
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        Orders = c.Orders.Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                Id = op.Id,
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                },
                OrderId = op.OrderId,
                Quantity = op.Quantity
            }).ToList(),
            PaidOnDate = o.PaidOnDate
        }).ToList()
    }).SingleOrDefault(c => c.Id == id) is CashierDTO cashier ? 
    Results.Ok(cashier) : 
    Results.NoContent();
});

app.MapPost("/cashiers", (CornerStoreDbContext db, Cashier cashier) => 
{
    db.Cashiers.Add(cashier);
    db.SaveChanges();
    return Results.Created($"/cashiers/{cashier.Id}", cashier);
});

//----------> Products <-------------

app.MapGet("/products", (string search, CornerStoreDbContext db) => 
{
    IQueryable<Product> query = db.Products
    .Include(p => p.Category);

    if (!string.IsNullOrEmpty(search))
    {
        query = query.Where(p => p.Category.CategoryName.ToLower()
        .Contains(search.ToLower()) 
        || p.ProductName.ToLower()
        .Contains(search.ToLower()));
    }

    return query.Select(p => new ProductDTO
    {
        Id = p.Id,
        ProductName = p.ProductName,
        Price = p.Price,
        Brand = p.Brand,
        CategoryId = p.CategoryId,
        Category = new CategoryDTO
        {
            Id = p.Category.Id,
            CategoryName = p.Category.CategoryName
        }
    });
});

app.MapPost("/products", (CornerStoreDbContext db, Product product) =>
{
    db.Products.Add(product);
    db.SaveChanges();
    return Results.Created($"/products/{product.Id}", product);
});

app.MapPut("/products/{id}", (int id, CornerStoreDbContext db, Product product) => 
{
    Product productToUpdate = db.Products.SingleOrDefault(p => p.Id == id);

    if (productToUpdate == null)
    {
        return Results.NoContent();
    }

    productToUpdate.ProductName = product.ProductName;
    productToUpdate.Price = product.Price;
    productToUpdate.Brand = product.Brand;
    productToUpdate.CategoryId = product.CategoryId;

    db.SaveChanges();
    return Results.NoContent();
});

app.MapGet("/products/popular", (CornerStoreDbContext db, int? amount) => 
{
    int productCount = amount ?? 5;
    
    var popularProducts = db.OrderProducts
    .GroupBy(op => op.ProductId)
    .Select(g => new
    {
        ProductId = g.Key,
        TotalQuantitySold = g.Sum(op => op.Quantity)
    })
    .OrderByDescending(x => x.TotalQuantitySold)
    .Take(productCount)
    .Join(db.Products
    .Include(p => p.Category),
    popular => popular.ProductId,
    product => product.Id,
    (popular, product) => new ProductDTO
    {
        Id = product.Id,
        ProductName = product.ProductName,
        Price = product.Price,
        Brand = product.Brand,
        CategoryId = product.CategoryId,
        Category = new CategoryDTO
        {
            Id = product.Category.Id,
            CategoryName = product.Category.CategoryName
        }
        
    });
    return Results.Ok(popularProducts);
});

//----------> Orders <-----------

app.MapGet("/orders", (CornerStoreDbContext db, string orderDate) =>
{
    IQueryable<Order> query = db.Orders
    .Include(o => o.Cashier)
    .Include(o => o.OrderProducts)
        .ThenInclude(op => op.Product)
        .ThenInclude(p => p.Category);

        if (!string.IsNullOrEmpty(orderDate))
        {
            var parsedDate = DateTime.Parse(orderDate);
            query = query.Where(o => o.PaidOnDate == parsedDate);
        }

        return query.Select(o => new OrderDTO
        {
            Id = o.Id,
            CashierId = o.CashierId,
            Cashier = new CashierDTO
            {
                Id = o.Cashier.Id,
                FirstName = o.Cashier.FirstName,
                LastName = o.Cashier.LastName
            },
            OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
            {
                Id = op.Id,
                ProductId = op.ProductId,
                Product = new ProductDTO
                {
                    Id = op.Product.Id,
                    ProductName = op.Product.ProductName,
                    Price = op.Product.Price,
                    Brand = op.Product.Brand,
                    CategoryId = op.Product.CategoryId,
                    Category = new CategoryDTO
                    {
                        Id = op.Product.Category.Id,
                        CategoryName = op.Product.Category.CategoryName
                    }
                },
                Quantity = op.Quantity
                
            }).ToList(),
            PaidOnDate = o.PaidOnDate
        });
});

app.MapGet("/orders/{id}", (CornerStoreDbContext db, int id) => 
{
    return db.Orders
    .Include(o => o.Cashier)
    .Include(o => o.OrderProducts)
    .ThenInclude(p => p.Product)
    .Select(o => new OrderDTO
    {
        Id = o.Id,
        CashierId = o.CashierId,
        Cashier = new CashierDTO
        {
            Id = o.Cashier.Id,
            FirstName = o.Cashier.FirstName,
            LastName = o.Cashier.LastName
        },
        OrderProducts = o.OrderProducts.Select(op => new OrderProductDTO
        {
            Id = op.Id,
            ProductId = op.Id,
            OrderId = op.OrderId,
            Quantity = op.Quantity,
            Product = new ProductDTO
            {
                 Id = op.Product.Id,
            ProductName = op.Product.ProductName,
            Price = op.Product.Price,
            Brand = op.Product.Brand,
            CategoryId = op.Product.CategoryId,
            Category = new CategoryDTO
            {
                Id = op.Product.Category.Id,
                CategoryName = op.Product.Category.CategoryName
            }
            }
        }).ToList(),
        PaidOnDate = o.PaidOnDate
    }).SingleOrDefault(o => o.Id == id) is OrderDTO order ? 
    Results.Ok(order) : 
    Results.NotFound();
});

app.MapDelete("/orders/{id}", (int id, CornerStoreDbContext db) => 
{ 
    Order orderToDelete = db.Orders.SingleOrDefault(o => o.Id == id);

    if(orderToDelete == null)
    {
        return Results.NotFound();
    }

    db.Orders.Remove(orderToDelete);
    db.SaveChanges();
    return Results.NoContent();
});

app.MapPost("/orders", (CornerStoreDbContext db, Order order) =>
{
 
    foreach (OrderProduct op in order.OrderProducts)
    {
        Product product = db.Products.Find(op.ProductId);
        if (product == null)
        {
            return Results.BadRequest();
        }
    }
    
    Order newOrder = new Order
    {
        CashierId = order.CashierId,
        OrderProducts = order.OrderProducts.Select(op => new OrderProduct
        {
            ProductId = op.ProductId,
            Quantity = op.Quantity
        }).ToList(),
            PaidOnDate = order.PaidOnDate
    };

    db.Orders.Add(newOrder);
    db.SaveChanges();
    return Results.Created($"/orders/{newOrder.Id}", newOrder);
});

app.Run();

//don't move or change this!
public partial class Program { }