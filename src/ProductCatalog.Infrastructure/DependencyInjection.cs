using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductCatalog.Application.Interfaces;
using ProductCatalog.Domain.Interfaces;
using ProductCatalog.Infrastructure.Data;
using ProductCatalog.Infrastructure.Repositories;
using ProductCatalog.Infrastructure.Services;

namespace ProductCatalog.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<ProductCatalogDbContext>(options =>
            options.UseInMemoryDatabase("ProductCatalogDb"));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddSingleton<ICategoryRepository, CategoryRepository>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddSingleton<ISearchCacheService>(sp => new SearchCacheService(TimeSpan.FromMinutes(5)));

        return services;
    }
}
