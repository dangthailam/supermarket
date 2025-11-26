using SuperMarket.Application;
using SuperMarket.Infrastructure;
using OfficeOpenXml;

// Set EPPlus license for version 8+
ExcelPackage.License.SetNonCommercialPersonal("SuperMarket App");

var builder = WebApplication.CreateBuilder(args);


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
            policy.WithOrigins("http://localhost:4200", "http://localhost:4201")
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

app.UseCors("AllowAngularApp");

app.UseAuthorization();

app.MapControllers();

app.Run();
