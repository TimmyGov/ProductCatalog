using Microsoft.EntityFrameworkCore;
using ProductCatalog.Infrastructure;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Application.Services.SearchEngine;
using ProductCatalog.Domain.Entities;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.API.Middleware;
using ProductCatalog.API.Converters;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Register custom JSON converter
        options.JsonSerializerOptions.Converters.Add(new CategoryTreeDtoConverter());
    });

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Infrastructure services (includes DbContext, repositories, and services)
builder.Services.AddInfrastructure();

// Register SearchEngine as Singleton with proper configuration
builder.Services.AddSingleton<SearchEngine<Product>>(sp =>
{
    var searchEngine = new SearchEngine<Product>(maxLevenshteinDistance: 3);
    
    // Configure search fields with weights
    searchEngine.AddSearchField(p => p.Name, weight: 2.0);
    searchEngine.AddSearchField(p => p.Description, weight: 1.0);
    searchEngine.AddSearchField(p => p.SKU, weight: 1.5);
    
    return searchEngine;
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Seed data
await SeedDataAsync(app);

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Register custom middleware manually (without UseMiddleware helper)
app.Use(async (context, next) =>
{
    var middleware = new RequestTimingMiddleware(
        next,
        app.Services.GetRequiredService<ILogger<RequestTimingMiddleware>>());
    
    await middleware.InvokeAsync(context);
});

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedDataAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ProductCatalogDbContext>();
    var categoryRepo = scope.ServiceProvider.GetRequiredService<ICategoryRepository>();

    // Ensure database is created
    await context.Database.EnsureCreatedAsync();

    // Check if data already exists (check in-memory category repository)
    var existingCategories = await categoryRepo.GetAll();
    if (existingCategories.Any())
    {
        return;
    }

    // Seed categories with hierarchical structure
    var categories = new[]
    {
        new Category { Id = 1, Name = "Electronics", Description = "Electronic devices and accessories", ParentCategoryId = null },
        new Category { Id = 2, Name = "Computers", Description = "Desktop and laptop computers", ParentCategoryId = 1 },
        new Category { Id = 3, Name = "Laptops", Description = "Portable computers", ParentCategoryId = 2 },
        new Category { Id = 4, Name = "Desktops", Description = "Desktop computers", ParentCategoryId = 2 },
        new Category { Id = 5, Name = "Smartphones", Description = "Mobile phones", ParentCategoryId = 1 },
        new Category { Id = 6, Name = "Clothing", Description = "Apparel and fashion items", ParentCategoryId = null },
        new Category { Id = 7, Name = "Men's Clothing", Description = "Clothing for men", ParentCategoryId = 6 },
        new Category { Id = 8, Name = "Women's Clothing", Description = "Clothing for women", ParentCategoryId = 6 },
    };

    // Add to both DbContext and CategoryRepository
    await context.Categories.AddRangeAsync(categories);
    await context.SaveChangesAsync();

    // Also add to the in-memory CategoryRepository
    foreach (var category in categories)
    {
        await categoryRepo.Add(category);
    }

    // Seed products
    var products = new[]
    {
        new Product
        {
            Id = 1,
            Name = "MacBook Pro 16\"",
            Description = "High-performance laptop with M3 chip",
            SKU = "MBP-16-M3",
            Price = 2499.99m,
            Quantity = 15,
            CategoryId = 3,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 2,
            Name = "Dell XPS 15",
            Description = "Premium Windows laptop",
            SKU = "DELL-XPS-15",
            Price = 1899.99m,
            Quantity = 20,
            CategoryId = 3,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 3,
            Name = "iPhone 15 Pro",
            Description = "Latest iPhone with titanium design",
            SKU = "IPH-15-PRO",
            Price = 999.99m,
            Quantity = 50,
            CategoryId = 5,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 4,
            Name = "Samsung Galaxy S24",
            Description = "Flagship Android smartphone",
            SKU = "SAM-S24",
            Price = 899.99m,
            Quantity = 40,
            CategoryId = 5,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 5,
            Name = "Gaming Desktop PC",
            Description = "High-end gaming computer with RTX 4090",
            SKU = "DESK-GAME-001",
            Price = 3499.99m,
            Quantity = 8,
            CategoryId = 4,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 6,
            Name = "Men's Casual Shirt",
            Description = "100% cotton casual shirt",
            SKU = "SHIRT-M-001",
            Price = 49.99m,
            Quantity = 100,
            CategoryId = 7,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 7,
            Name = "Women's Summer Dress",
            Description = "Floral pattern summer dress",
            SKU = "DRESS-W-001",
            Price = 79.99m,
            Quantity = 75,
            CategoryId = 8,
            CreatedAt = DateTime.UtcNow
        },
        new Product
        {
            Id = 8,
            Name = "Office Desktop",
            Description = "Business desktop computer",
            SKU = "DESK-OFF-001",
            Price = 899.99m,
            Quantity = 25,
            CategoryId = 4,
            CreatedAt = DateTime.UtcNow
        }
    };

    await context.Products.AddRangeAsync(products);
    await context.SaveChangesAsync();
}
