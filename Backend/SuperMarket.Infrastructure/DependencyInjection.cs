using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Entities;
using SuperMarket.Infrastructure.Configuration;
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
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection"),
                npgsqlOptions => npgsqlOptions
                    .EnableRetryOnFailure()
                    .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)
            ));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IRepository<Product>, Repository<Product>>();
        
        // Add Infrastructure services
        services.AddScoped<IExcelImportService, ExcelImportService>();
        services.AddScoped<ITransactionService, TransactionService>();
        services.AddScoped<ISkuGeneratorService, SkuGeneratorService>();

        // Configure Supabase
        services.AddSingleton(sp =>
        {
            var supabaseUrl = configuration["Supabase:Url"];
            var supabaseKey = configuration["Supabase:Key"];
            
            if (string.IsNullOrEmpty(supabaseUrl) || string.IsNullOrEmpty(supabaseKey))
            {
                throw new InvalidOperationException("Supabase configuration is missing or invalid");
            }
            
            var options = new Supabase.SupabaseOptions
            {
                AutoConnectRealtime = false
            };
            
            return new Supabase.Client(supabaseUrl, supabaseKey, options);
        });

        return services;
    }
}
