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

            // Add HttpContextAccessor for user context
            builder.Services.AddHttpContextAccessor();

            // Add JWT Authentication
            builder.Services.AddAuthentication("Bearer")
                .AddJwtBearer("Bearer", options =>
                {
                    var supabaseUrl = builder.Configuration["Supabase:Url"];

                    if (string.IsNullOrEmpty(supabaseUrl))
                    {
                        throw new InvalidOperationException("Supabase URL is required for authentication");
                    }

                    // Supabase uses ES256 (asymmetric) signing with JWKS endpoint
                    // Configure to use Supabase's JSON Web Key Set (JWKS) endpoint
                    options.RequireHttpsMetadata = false; // Allow HTTP in development
                    options.MetadataAddress = $"{supabaseUrl}/auth/v1/.well-known/jwks.json";
                    
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidIssuer = $"{supabaseUrl}/auth/v1",
                        ValidateAudience = true,
                        ValidAudience = "authenticated",
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    // Configure the configuration manager to fetch JWKS
                    options.ConfigurationManager = new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                        $"{supabaseUrl}/auth/v1/.well-known/jwks.json",
                        new JwksRetriever(),
                        new Microsoft.IdentityModel.Protocols.HttpDocumentRetriever());
                    
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnAuthenticationFailed = context =>
                        {
                            Log.Warning("Authentication failed: {Error}", context.Exception.Message);
                            if (context.Exception.InnerException != null)
                            {
                                Log.Warning("Inner exception: {InnerError}", context.Exception.InnerException.Message);
                            }
                            return Task.CompletedTask;
                        },
                        OnTokenValidated = context =>
                        {
                            var userId = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                ?? context.Principal?.FindFirst("sub")?.Value;
                            Log.Debug("Token validated for user: {UserId}", userId);
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization();

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

            // Authentication & Authorization middleware
            app.UseAuthentication();
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

// Custom JWKS retriever for Supabase
public class JwksRetriever : Microsoft.IdentityModel.Protocols.IConfigurationRetriever<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>
{
    public async Task<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration> GetConfigurationAsync(
        string address, 
        Microsoft.IdentityModel.Protocols.IDocumentRetriever retriever, 
        CancellationToken cancel)
    {
        var jwksJson = await retriever.GetDocumentAsync(address, cancel);
        var jwks = new Microsoft.IdentityModel.Tokens.JsonWebKeySet(jwksJson);
        
        var config = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration();
        foreach (var key in jwks.Keys)
        {
            config.SigningKeys.Add(key);
        }
        
        return config;
    }
}
