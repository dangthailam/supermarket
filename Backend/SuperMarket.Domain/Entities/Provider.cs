using SuperMarket.Domain.Common;
using SuperMarket.Domain.ValueObjects;

public class Provider : Entity
{
    public string Name { get; private set; }

    public string Code { get; private set; }

    public string Phone { get; private set; }

    public string Email { get; private set; }

    public Address Address { get; private set; }

    public string Note { get; private set; }

    public string CompanyName { get; private set; }

    public string TaxNumber { get; private set; }

    public Provider(string name, string code, string phone, string email, Address address, string note, string companyName, string taxNumber)
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
}