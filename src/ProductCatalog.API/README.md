# Product Catalog API Layer

This document describes the API layer implementation for the Product Catalog Management System.

## Implemented Features

### 1. Controllers

#### ProductsController (`/api/products`)
- **GET /api/products** - List products with:
  - Pagination: `?page=1&pageSize=10` (default: page=1, pageSize=10)
  - Filtering: `?categoryId=1&minPrice=100&maxPrice=1000`
  - Search: `?searchTerm=laptop` (uses SearchEngine with fuzzy matching)
  - Pattern matching validation for all query parameters

- **GET /api/products/{id}** - Get single product by ID

- **POST /api/products** - Create new product
  - Pattern matching validation for product properties

- **PUT /api/products/{id}** - Update existing product

- **DELETE /api/products/{id}** - Delete product
  - **Manual Model Binding Demonstration**: Requires `X-Confirm-Delete: true` header
  - Header is read manually from `Request.Headers` without automatic binding

#### CategoriesController (`/api/categories`)
- **GET /api/categories** - Get flat list of all categories

- **GET /api/categories/tree** - Get hierarchical category tree
  - Uses custom JSON converter for serialization

- **POST /api/categories** - Create new category
  - **Manual Model Binding Demonstration**: Reads request body manually using StreamReader
  - Parses JSON manually without [FromBody] attribute
  - Pattern matching validation for category properties

### 2. Pattern Matching

Pattern matching is used extensively for validation:

**In ProductsController:**
```csharp
var validationResult = (page, pageSize, minPrice, maxPrice) switch
{
    ( <= 0, _, _, _) => "Page must be greater than 0",
    (_, <= 0, _, _) => "PageSize must be greater than 0",
    (_, > 100, _, _) => "PageSize cannot exceed 100",
    (_, _, < 0, _) => "MinPrice cannot be negative",
    (_, _, _, < 0) => "MaxPrice cannot be negative",
    (_, _, not null, not null) when minPrice > maxPrice => "MinPrice cannot be greater than MaxPrice",
    _ => null
};
```

**Product validation:**
```csharp
var validationError = createProductDto switch
{
    { Name: null or "" } => "Product name is required",
    { Name.Length: > 200 } => "Product name cannot exceed 200 characters",
    { SKU: null or "" } => "SKU is required",
    { Price: < 0 } => "Price cannot be negative",
    { Quantity: < 0 } => "Quantity cannot be negative",
    { CategoryId: <= 0 } => "Valid category ID is required",
    _ => null
};
```

**Category validation:**
```csharp
var validationError = createCategoryDto switch
{
    { Name: null or "" } => "Category name is required",
    { Name.Length: > 100 } => "Category name cannot exceed 100 characters",
    { Description: null or "" } => "Category description is required",
    { ParentCategoryId: < 0 } => "Parent category ID must be positive or null",
    _ => null
};
```

### 3. Custom Middleware

**RequestTimingMiddleware** - Logs request timing information
- Implemented manually without UseMiddleware<T> helper
- Uses InvokeAsync pattern
- Logs:
  - Request start time
  - Request end time  
  - Duration in milliseconds
  - HTTP method and path
  - Response status code

**Manual registration in Program.cs:**
```csharp
app.Use(async (context, next) =>
{
    var middleware = new RequestTimingMiddleware(
        next,
        app.Services.GetRequiredService<ILogger<RequestTimingMiddleware>>());
    
    await middleware.InvokeAsync(context);
});
```

### 4. Custom JSON Serialization

**CategoryTreeDtoConverter** - Custom JsonConverter for CategoryTreeDto
- Handles hierarchical serialization/deserialization
- Properly serializes nested children
- Handles null parent categories
- Registered in Program.cs:
```csharp
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new CategoryTreeDtoConverter());
    });
```

### 5. Dependency Injection

**Service Registration in Program.cs:**

```csharp
// Infrastructure services (Scoped: DbContext, Repositories, Services)
builder.Services.AddInfrastructure();

// SearchEngine as Singleton with field configuration
builder.Services.AddSingleton<SearchEngine<Product>>(sp =>
{
    var searchEngine = new SearchEngine<Product>(maxLevenshteinDistance: 3);
    searchEngine.AddSearchField(p => p.Name, weight: 2.0);
    searchEngine.AddSearchField(p => p.Description, weight: 1.0);
    searchEngine.AddSearchField(p => p.SKU, weight: 1.5);
    return searchEngine;
});
```

**DI Lifetimes Demonstrated:**
- **Singleton**: SearchEngine<Product> - Single instance for the application lifetime
- **Scoped**: IProductService, ICategoryService, DbContext - One instance per request
- **Transient**: Not used in this implementation (but understood)

**Controller Injection:**
```csharp
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly SearchEngine<Product> _searchEngine;

    public ProductsController(IProductService productService, SearchEngine<Product> searchEngine)
    {
        _productService = productService;
        _searchEngine = searchEngine;
    }
}
```

### 6. Configuration

**Program.cs includes:**
- Controllers with custom JSON options
- Swagger/OpenAPI for API documentation
- CORS configuration (AllowAll policy for development)
- In-memory database via AddInfrastructure()
- Seed data for testing (8 products, 8 categories in hierarchical structure)

### 7. Seed Data

The application seeds data on startup:
- 8 categories in a hierarchical structure:
  - Electronics > Computers > Laptops/Desktops
  - Electronics > Smartphones
  - Clothing > Men's Clothing / Women's Clothing
- 8 products across different categories

## Testing the API

### Get all products
```bash
curl http://localhost:5000/api/products
```

### Get products with filters
```bash
curl "http://localhost:5000/api/products?page=1&pageSize=5&minPrice=500&maxPrice=2000"
```

### Search products
```bash
curl "http://localhost:5000/api/products?searchTerm=laptop"
```

### Get single product
```bash
curl http://localhost:5000/api/products/1
```

### Create product
```bash
curl -X POST http://localhost:5000/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"New Product","description":"Description","sku":"SKU-001","price":99.99,"quantity":10,"categoryId":3}'
```

### Update product
```bash
curl -X PUT http://localhost:5000/api/products/1 \
  -H "Content-Type: application/json" \
  -d '{"id":1,"name":"Updated Product","description":"Updated","sku":"SKU-001","price":199.99,"quantity":20,"categoryId":3}'
```

### Delete product (with manual binding header)
```bash
curl -X DELETE http://localhost:5000/api/products/1 \
  -H "X-Confirm-Delete: true"
```

### Get all categories
```bash
curl http://localhost:5000/api/categories
```

### Get category tree
```bash
curl http://localhost:5000/api/categories/tree
```

### Create category (manual model binding)
```bash
curl -X POST http://localhost:5000/api/categories \
  -H "Content-Type: application/json" \
  -d '{"name":"New Category","description":"Description","parentCategoryId":1}'
```

## Key Requirements Met

✅ **Controllers**: ProductsController and CategoriesController with all required endpoints
✅ **Manual Model Binding**: DELETE in ProductsController (header), POST in CategoriesController (body)
✅ **Pattern Matching**: Extensive use in validation logic
✅ **Custom Middleware**: RequestTimingMiddleware implemented manually
✅ **Custom JSON Converter**: CategoryTreeDtoConverter for hierarchical data
✅ **Program.cs Configuration**: All services registered with proper lifetimes
✅ **Dependency Injection**: Proper injection with understanding of lifetimes
✅ **Seed Data**: Categories and products for testing
✅ **Nullable Reference Types**: Used throughout the codebase
