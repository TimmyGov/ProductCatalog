# Product Catalog Management System

A full-stack Product Catalog Management System built with **Clean Architecture** principles, featuring a **C# .NET 10 Web API** backend and an **Angular 16** frontend.

## 🎯 Overview

This application allows administrators to manage product inventory through a web interface with comprehensive CRUD operations, search functionality with fuzzy matching, and hierarchical category management.

## 📋 Table of Contents

- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Project Structure](#project-structure)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Architecture](#architecture)
- [Advanced Features](#advanced-features)

## ✨ Features

### Backend (C# .NET)
- ✅ **Clean Architecture** with 4 layers (Domain, Application, Infrastructure, API)
- ✅ **Custom Repository Pattern** with generic base implementation
- ✅ **ProductSearchEngine** with fuzzy matching using Levenshtein distance (no external NuGet packages)
- ✅ **Custom LINQ Extension Methods** for filtering and searching
- ✅ **Record Types (C# 9+)** for all DTOs
- ✅ **Pattern Matching** for request validation
- ✅ **Nullable Reference Types** throughout the codebase
- ✅ **Custom Middleware** implemented from scratch
- ✅ **Caching Layer** using Dictionary with expiration logic
- ✅ **Category Tree Structure** supporting parent-child relationships
- ✅ **IComparable Implementation** for custom sorting
- ✅ **Manual Model Binding** examples
- ✅ **Custom JSON Serialization** for CategoryTreeDto
- ✅ **Dependency Injection** with proper lifetime management

### Frontend (Angular)
- ✅ **Angular 16** with standalone components
- ✅ **TypeScript Strict Mode** for type safety
- ✅ **Reactive Forms** with comprehensive validation
- ✅ **RxJS** for reactive programming with debounce
- ✅ **Search Functionality** with real-time filtering
- ✅ **Category Filtering** dropdown
- ✅ **Pagination** controls
- ✅ **Responsive Design** mobile-friendly
- ✅ **Loading Indicators** and error handling
- ✅ **CRUD Operations** for products and categories

## 🛠 Technology Stack

### Backend
- **.NET 10.0** SDK
- **ASP.NET Core** Web API
- **Entity Framework Core** (in-memory database)
- **Swashbuckle** for Swagger/OpenAPI
- **xUnit** for testing
- **Moq** for mocking

### Frontend
- **Angular 16** (standalone components)
- **TypeScript 5.x** (strict mode)
- **RxJS** for reactive programming
- **Jasmine/Karma** for testing
- **HttpClient** for API communication

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

### Backend
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) or later
- Any IDE: Visual Studio 2022, Visual Studio Code, or JetBrains Rider

### Frontend
- [Node.js](https://nodejs.org/) v18.x or later
- [npm](https://www.npmjs.com/) v9.x or later
- Angular CLI: `npm install -g @angular/cli@16`

## 🚀 Getting Started

### Backend Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/TimmyGov/ProductCatalog.git
   cd ProductCatalog
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
   ```bash
   dotnet build
   ```

4. **Run the tests**
   ```bash
   dotnet test
   ```
   
   Expected output: `Passed! - Failed: 0, Passed: 36, Skipped: 0`

5. **Run the API**
   ```bash
   cd src/ProductCatalog.API
   dotnet run
   ```
   
   The API will start on `https://localhost:5001` (or `http://localhost:5000`)

6. **Access Swagger UI**
   
   Navigate to: `https://localhost:5001/swagger` (or `http://localhost:5000/swagger`)

### Frontend Setup

1. **Navigate to the frontend directory**
   ```bash
   cd product-catalog-ui
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Configure API URL (if needed)**
   
   Edit `src/environments/environment.ts` to set the API base URL:
   ```typescript
   export const environment = {
     apiUrl: 'http://localhost:5000'  // or your API URL
   };
   ```

4. **Run the development server**
   ```bash
   npm start
   ```
   
   Or:
   ```bash
   ng serve
   ```
   
   The app will be available at `http://localhost:4200`

5. **Build for production**
   ```bash
   npm run build
   ```
   
   Or:
   ```bash
   ng build --configuration production
   ```
   
   Production files will be in the `dist/` folder

6. **Run tests**
   ```bash
   npm test
   ```

## 📁 Project Structure

```
ProductCatalog/
├── src/
│   ├── ProductCatalog.Domain/          # Core entities, interfaces, exceptions
│   │   ├── Entities/                   # Product, Category
│   │   ├── Interfaces/                 # IRepository, IProductRepository, ICategoryRepository
│   │   └── Exceptions/                 # EntityNotFoundException, DuplicateEntityException
│   │
│   ├── ProductCatalog.Application/     # Use cases, DTOs, business logic
│   │   ├── DTOs/                       # Record types for data transfer
│   │   ├── Interfaces/                 # IProductService, ICategoryService
│   │   ├── Extensions/                 # Custom LINQ extensions
│   │   └── Services/                   # SearchEngine<T> with fuzzy matching
│   │
│   ├── ProductCatalog.Infrastructure/  # Data access, repositories
│   │   ├── Data/                       # ProductCatalogDbContext
│   │   ├── Repositories/               # Repository implementations
│   │   ├── Services/                   # Service implementations
│   │   └── DependencyInjection.cs
│   │
│   └── ProductCatalog.API/             # REST API, controllers, middleware
│       ├── Controllers/                # ProductsController, CategoriesController
│       ├── Middleware/                 # RequestTimingMiddleware
│       ├── Converters/                 # CategoryTreeDtoConverter
│       └── Program.cs                  # App configuration
│
├── tests/
│   └── ProductCatalog.Tests/           # Unit tests
│       ├── ProductSearchEngineTests.cs  # 23 tests
│       └── ProductServiceTests.cs       # 13 tests
│
└── product-catalog-ui/                 # Angular frontend
    ├── src/
    │   ├── app/
    │   │   ├── components/             # UI components (6 standalone)
    │   │   ├── services/               # HTTP services (3 services)
    │   │   └── models/                 # TypeScript interfaces
    │   └── environments/               # Environment config
    └── ...
```

## 📚 API Documentation

### Base URL
```
http://localhost:5000/api
```

### Products Endpoints

#### Get All Products (with filtering and search)
```http
GET /api/products?page=1&pageSize=10&categoryId=1&minPrice=0&maxPrice=1000&searchTerm=laptop
```

**Query Parameters:**
- `page` (int, optional): Page number (default: 1)
- `pageSize` (int, optional): Items per page (default: 10)
- `categoryId` (int, optional): Filter by category
- `minPrice` (decimal, optional): Minimum price filter
- `maxPrice` (decimal, optional): Maximum price filter
- `searchTerm` (string, optional): Search by name with fuzzy matching

**Response:**
```json
{
  "items": [...],
  "totalCount": 50,
  "page": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

#### Get Product by ID
```http
GET /api/products/{id}
```

#### Create Product
```http
POST /api/products
Content-Type: application/json

{
  "name": "Laptop",
  "description": "High-performance laptop",
  "sku": "LAP-001",
  "price": 999.99,
  "quantity": 10,
  "categoryId": 1
}
```

#### Update Product
```http
PUT /api/products/{id}
Content-Type: application/json

{
  "id": 1,
  "name": "Updated Laptop",
  "description": "Updated description",
  "sku": "LAP-001",
  "price": 899.99,
  "quantity": 15,
  "categoryId": 1
}
```

#### Delete Product
```http
DELETE /api/products/{id}
X-Confirm-Delete: true
```

**Note:** Requires `X-Confirm-Delete: true` header (manual model binding example)

### Categories Endpoints

#### Get All Categories (flat list)
```http
GET /api/categories
```

#### Get Category Tree (hierarchical)
```http
GET /api/categories/tree
```

**Response Example:**
```json
[
  {
    "id": 1,
    "name": "Electronics",
    "description": "Electronic devices",
    "parentCategoryId": null,
    "children": [
      {
        "id": 2,
        "name": "Computers",
        "description": "Computer products",
        "parentCategoryId": 1,
        "children": []
      }
    ]
  }
]
```

#### Create Category
```http
POST /api/categories
Content-Type: application/json

{
  "name": "New Category",
  "description": "Category description",
  "parentCategoryId": 1
}
```

**Note:** This endpoint uses manual model binding (reads request body via StreamReader)

### Swagger/OpenAPI

Full interactive API documentation is available at:
```
https://localhost:5001/swagger
```

## 🧪 Testing

### Backend Tests

Run all tests:
```bash
dotnet test
```

Run with verbose output:
```bash
dotnet test --verbosity normal
```

Run specific test file:
```bash
dotnet test --filter "FullyQualifiedName~ProductSearchEngineTests"
```

**Test Coverage:**
- ProductSearchEngine: 23 tests covering fuzzy matching, scoring, performance
- ProductService: 13 tests covering CRUD operations and filtering
- Total: 36 tests, all passing

### Frontend Tests

Run tests:
```bash
cd product-catalog-ui
npm test
```

Run tests with coverage:
```bash
npm test -- --code-coverage
```

**Test Coverage:**
- ProductService: Tests for HTTP operations
- Components: Basic rendering tests
- Total: 10 tests, all passing

## 🏗 Architecture

This application follows **Clean Architecture** principles:

### Layers

1. **Domain Layer** (innermost):
   - Contains business entities and core business rules
   - No dependencies on other layers
   - Entity interfaces, domain exceptions

2. **Application Layer**:
   - Contains application business rules
   - Depends only on Domain layer
   - DTOs, service interfaces, search engine

3. **Infrastructure Layer**:
   - Contains implementations of interfaces defined in Application layer
   - Repositories, data access, external services
   - Depends on Domain and Application layers

4. **Presentation Layer** (API):
   - Web API controllers, middleware
   - Depends on Application and Infrastructure layers
   - Entry point for the application

### Design Principles

- **Dependency Inversion**: High-level modules don't depend on low-level modules
- **Separation of Concerns**: Each layer has a specific responsibility
- **SOLID Principles**: Applied throughout the codebase
- **Repository Pattern**: Abstracts data access
- **Dependency Injection**: Used for loose coupling

## 🚀 Advanced Features

### 1. ProductSearchEngine with Fuzzy Matching

The custom search engine uses:
- **Levenshtein Distance** algorithm for fuzzy matching
- **Multi-field weighted scoring** (Name: 3x, Description: 1x, SKU: 2x)
- **Performance optimization** for 10,000+ products
- **Pure .NET BCL** implementation (no external packages)

Example usage:
```csharp
var searchEngine = new SearchEngine<Product>();
searchEngine.AddFieldConfiguration("Name", 3.0);
searchEngine.AddFieldConfiguration("Description", 1.0);
searchEngine.AddFieldConfiguration("SKU", 2.0);

var results = searchEngine.Search(products, "lptop"); // Finds "laptop"
```

### 2. Custom LINQ Extension Methods

```csharp
var filteredProducts = products
    .FilterByCategory(categoryId: 1)
    .FilterByPrice(minPrice: 100, maxPrice: 500)
    .SearchByName("laptop")
    .SortByPrice(ascending: false);
```

### 3. Pattern Matching for Validation

```csharp
var validationResult = (page, pageSize, searchTerm) switch
{
    ( <= 0, _, _) => BadRequest("Page must be greater than 0"),
    (_, <= 0, _) => BadRequest("Page size must be greater than 0"),
    (_, _, null or "") => BadRequest("Search term cannot be empty"),
    _ => null
};
```

### 4. Custom Middleware

Request timing middleware logs the duration of each request:
```csharp
app.Use(async (context, next) => {
    var startTime = DateTime.UtcNow;
    await next(context);
    var duration = DateTime.UtcNow - startTime;
    logger.LogInformation($"Request to {context.Request.Path} took {duration.TotalMilliseconds}ms");
});
```

### 5. Caching with Expiration

```csharp
var cacheService = new SearchCacheService<string, List<ProductDto>>(
    timeToLive: TimeSpan.FromMinutes(5)
);

if (!cacheService.TryGet(cacheKey, out var cachedResults))
{
    cachedResults = await SearchProducts(searchTerm);
    cacheService.Set(cacheKey, cachedResults);
}
```

## 🔒 Security

- **Nullable reference types** enabled to prevent null reference exceptions
- **Input validation** with pattern matching
- **Parameter validation** in all endpoints
- **CodeQL security analysis** passed
- **No external dependencies** in critical components (SearchEngine)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is created for demonstration purposes.

## 📧 Contact

For questions or feedback, please open an issue on GitHub.

## 🙏 Acknowledgments

- Clean Architecture principles by Robert C. Martin
- Angular team for the excellent framework
- .NET team for ASP.NET Core and Entity Framework Core
