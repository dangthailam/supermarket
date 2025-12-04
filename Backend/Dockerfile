# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY SuperMarket.Domain/SuperMarket.Domain.csproj SuperMarket.Domain/
COPY SuperMarket.Application/SuperMarket.Application.csproj SuperMarket.Application/
COPY SuperMarket.Infrastructure/SuperMarket.Infrastructure.csproj SuperMarket.Infrastructure/
COPY SuperMarket.API/SuperMarket.API.csproj SuperMarket.API/

# Restore dependencies
RUN dotnet restore SuperMarket.API/SuperMarket.API.csproj

# Copy all source code
COPY . .

# Build and publish
RUN dotnet publish SuperMarket.API/SuperMarket.API.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "SuperMarket.API.dll"]
