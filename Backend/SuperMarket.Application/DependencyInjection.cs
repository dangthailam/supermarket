using Microsoft.Extensions.DependencyInjection;
using SuperMarket.Application.Services;
using System.Reflection;

namespace SuperMarket.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<IProductService, ProductService>();
        
        return services;
    }
}
