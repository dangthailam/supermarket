using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SuperMarket.Application.Interfaces;

namespace SuperMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ICurrentUserService _currentUserService;

    public AuthController(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Get information about the currently authenticated user
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Unauthorized(new { message = "User is not authenticated" });
        }

        return Ok(new
        {
            userId = _currentUserService.UserId,
            email = _currentUserService.UserEmail,
            name = _currentUserService.UserName,
            isAuthenticated = _currentUserService.IsAuthenticated
        });
    }

    /// <summary>
    /// Health check endpoint that doesn't require authentication
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
