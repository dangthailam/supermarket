using SuperMarket.Domain.Common;
using SuperMarket.Domain.ValueObjects;

namespace SuperMarket.Domain.Entities;

public class Customer : Entity
{
    public string Name { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public Address? Address { get; private set; }
    public DateTime? DateOfBirth { get; private set; }
    public string? Gender { get; private set; } // "Male", "Female", "Other"
    public string? CustomerType { get; private set; } // "Regular", "VIP", "Wholesale"
    public bool IsActive { get; private set; } = true;

    // Parameterless constructor for EF Core
    private Customer()
    {
    }

    public Customer(string name, string email, string? phone, Address? address, DateTime? dateOfBirth, string? gender, string? customerType)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        CustomerType = customerType;
        IsActive = true;
    }

    public void Update(string name, string email, string? phone, Address? address, DateTime? dateOfBirth, string? gender, string? customerType)
    {
        Name = name;
        Email = email;
        Phone = phone;
        Address = address;
        DateOfBirth = dateOfBirth;
        Gender = gender;
        CustomerType = customerType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
