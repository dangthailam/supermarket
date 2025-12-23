using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SuperMarket.Application.Interfaces;

namespace SuperMarket.Infrastructure.Services;

/// <summary>
/// Service to access the current authenticated user's information from HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                              ?? _httpContextAccessor.HttpContext?.User?.FindFirst("sub")?.Value;

            return userIdClaim != null && Guid.TryParse(userIdClaim, out var userId) 
                ? userId 
                : null;
        }
    }

    public string? UserEmail => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Email)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("email")?.Value;

    public string? UserName => 
        _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Name)?.Value
        ?? _httpContextAccessor.HttpContext?.User?.FindFirst("name")?.Value
        ?? _httpContextAccessor.HttpContext?.User?.Identity?.Name;

    public bool IsAuthenticated => 
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
