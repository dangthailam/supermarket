namespace SuperMarket.Application.Interfaces;

/// <summary>
/// Service to access information about the current authenticated user
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's email
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Gets the current user's name
    /// </summary>
    string? UserName { get; }

    /// <summary>
    /// Indicates whether a user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }
}
