using SuperMarket.Application;
using SuperMarket.Infrastructure;
using OfficeOpenXml;
using Serilog;

namespace SuperMarket.API;

public class Program
{
    public static void Main(string[] args)
    {
        // Set EPPlus license for version 8+
        ExcelPackage.License.SetNonCommercialPersonal("SuperMarket App");

        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", Serilog.Events.LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.File(
                "logs/supermarket-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .CreateLogger();

        try
        {
            Log.Information("Starting SuperMarket API...");

            var builder = WebApplication.CreateBuilder(args);
            
            // Add Serilog to the service collection
            builder.Host.UseSerilog();

            builder.Services.AddApplication();

            builder.Services.AddInfrastructure(builder.Configuration);

            // Add repositories and services

            // Add controllers
            builder.Services.AddControllers();

            // Add CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAngularApp",
                    policy =>
                    {
                        var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
                            ?? new[] { "http://localhost:4222" };
                        
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    });
            });

            // Add response compression for performance
            builder.Services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            // Add memory cache
            builder.Services.AddMemoryCache();

            // Add Swagger/OpenAPI
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "SuperMarket API", Version = "v1" });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SuperMarket API v1"));
            }

            app.UseHttpsRedirection();

            app.UseResponseCompression();

            // Enable static files for uploads
            app.UseStaticFiles();

            app.UseCors("AllowAngularApp");

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
