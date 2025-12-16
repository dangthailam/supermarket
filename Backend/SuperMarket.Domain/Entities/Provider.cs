using SuperMarket.Domain.Common;
using SuperMarket.Domain.ValueObjects;

namespace SuperMarket.Domain.Entities;

public class Provider : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Code { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public Address? Address { get; private set; }
    public string? Note { get; private set; }
    public string? CompanyName { get; private set; }
    public string? TaxNumber { get; private set; }
    
    public ICollection<Purchase> Purchases { get; private set; } = [];

    private Provider()
    {
    }

    public Provider(string name, string code, string? phone, string? email, Address? address, string? note, string? companyName, string? taxNumber)
    {
        Name = name;
        Code = code;
        Phone = phone;
        Email = email;
        Address = address;
        Note = note;
        CompanyName = companyName;
        TaxNumber = taxNumber;
    }

    /// <summary>
    /// Updates provider information
    /// </summary>
    public void UpdateProvider(string name, string code, string? phone, string? email, Address? address, string? note, string? companyName, string? taxNumber)
    {
        Name = name;
        Code = code;
        Phone = phone;
        Email = email;
        Address = address;
        Note = note;
        CompanyName = companyName;
        TaxNumber = taxNumber;
    }

    /// <summary>
    /// Generates a unique provider code from the provider name
    /// </summary>
    public static string GenerateCode(string providerName)
    {
        // Generate code from provider name: take first 3 letters + timestamp
        var prefix = new string(providerName.Where(char.IsLetterOrDigit).Take(3).ToArray()).ToUpper();
        if (prefix.Length < 3) prefix = prefix.PadRight(3, 'X');
        
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString().Substring(5);
        return $"{prefix}-{timestamp}";
    }
}