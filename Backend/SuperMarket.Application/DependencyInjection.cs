using Microsoft.Extensions.DependencyInjection;
using SuperMarket.Application.Interfaces;
using SuperMarket.Application.Services;
using System.Reflection;

namespace SuperMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProviderService, ProviderService>();
        services.AddScoped<ICustomerService, CustomerService>();
        
        return services;
    }
}
