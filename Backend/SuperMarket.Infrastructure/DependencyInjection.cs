using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Entities;
using SuperMarket.Infrastructure.Data;
using SuperMarket.Infrastructure.Repositories;
using SuperMarket.Infrastructure.Services;
using System.Reflection;

namespace SuperMarket.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Add DbContext
        services.AddDbContext<SuperMarketDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                sqlOptions => sqlOptions.EnableRetryOnFailure()
            ));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepository<Product>, Repository<Product>>();
        
        // Add Infrastructure services
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<ITransactionService, TransactionService>();

        return services;
    }
}
