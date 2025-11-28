namespace SuperMarket.Application.DTOs;

public class ProviderDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Note { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string ContactName { get; set; } = string.Empty;
}

public class CreateProviderDto
{
    public string Name { get; set; } = string.Empty;
    public string? Phone { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Note { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? ContactName { get; set; }
}

public class UpdateProviderDto
{
    public string? Name { get; set; }
    public string? Code { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public string? District { get; set; }
    public string? City { get; set; }
    public string? Note { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxNumber { get; set; }
    public string? ContactName { get; set; }
}
