using SuperMarket.Domain.Common;

namespace SuperMarket.Domain.ValueObjects;

public sealed class Address : ValueObject
{
    public string AddressLine { get; }
    public string District { get; }
    public string City { get; }

    public Address(string addressLine, string district, string city)
    {
        if (string.IsNullOrWhiteSpace(addressLine))
            throw new ArgumentException("Address cannot be empty.", nameof(addressLine));
        
        if (string.IsNullOrWhiteSpace(district))
            throw new ArgumentException("District cannot be empty.", nameof(district));
        
        if (string.IsNullOrWhiteSpace(city))
            throw new ArgumentException("City cannot be empty.", nameof(city));

        AddressLine = addressLine;
        District = district;
        City = city;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AddressLine;
        yield return District;
        yield return City;
    }

    public override string ToString()
    {
        return $"{AddressLine}, {District}, {City}";
    }
}
