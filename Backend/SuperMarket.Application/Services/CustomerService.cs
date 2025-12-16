using SuperMarket.Application.DTOs;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Common;
using SuperMarket.Domain.Entities;
using SuperMarket.Domain.ValueObjects;

namespace SuperMarket.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;

    public CustomerService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto)
    {
        // Validate email uniqueness if provided
        if (Customer.ValidateEmail(dto.Email))
        {
            var existingCustomer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (existingCustomer != null)
                throw new InvalidOperationException($"Customer with email '{dto.Email}' already exists.");
        }

        var address = new Address(
            dto.Address ?? "Not specified",
            dto.District ?? "Not specified",
            dto.City ?? "Not specified"
        );

        var customer = new Customer(
            dto.Name,
            dto.Email,
            dto.Phone,
            address,
            dto.DateOfBirth,
            dto.Gender,
            dto.CustomerType ?? "Regular"
        );

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return null;

        return MapToDto(customer);
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllAsync();
        return customers.Select(c => MapToDto(c)).ToList();
    }

    public async Task<PaginatedResult<CustomerDto>> GetCustomersPagedAsync(PaginationParams paginationParams)
    {
        var allCustomers = await _unitOfWork.Customers.GetAllAsync();
        var filtered = allCustomers.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            filtered = filtered.Where(c =>
                c.Name.ToLower().Contains(searchTerm) ||
                c.Email.ToLower().Contains(searchTerm) ||
                (c.Phone != null && c.Phone.ToLower().Contains(searchTerm))
            );
        }

        // Apply sorting
        IOrderedQueryable<Customer> sorted = paginationParams.SortBy?.ToLower() switch
        {
            "name" => paginationParams.SortDescending
                ? filtered.OrderByDescending(c => c.Name)
                : filtered.OrderBy(c => c.Name),
            "email" => paginationParams.SortDescending
                ? filtered.OrderByDescending(c => c.Email)
                : filtered.OrderBy(c => c.Email),
            "createdat" => paginationParams.SortDescending
                ? filtered.OrderByDescending(c => c.CreatedAt)
                : filtered.OrderBy(c => c.CreatedAt),
            _ => filtered.OrderBy(c => c.Name)
        };

        var count = sorted.Count();
        var customers = sorted
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var customerDtos = customers.Select(c => MapToDto(c)).ToList();

        return new PaginatedResult<CustomerDto>(customerDtos, count, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return null;

        // Validate email uniqueness if changed
        if (Customer.ValidateEmail(dto.Email) && dto.Email != customer.Email)
        {
            var existingCustomer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Email == dto.Email);
            if (existingCustomer != null)
                throw new InvalidOperationException($"Customer with email '{dto.Email}' already exists.");
        }

        var address = customer.Address;
        if (!string.IsNullOrWhiteSpace(dto.Address) || !string.IsNullOrWhiteSpace(dto.District) || !string.IsNullOrWhiteSpace(dto.City))
        {
            address = new Address(
                dto.Address ?? customer.Address?.AddressLine ?? "Not specified",
                dto.District ?? customer.Address?.District ?? "Not specified",
                dto.City ?? customer.Address?.City ?? "Not specified"
            );
        }

        var name = dto.Name ?? customer.Name;
        var email = dto.Email ?? customer.Email;
        var phone = dto.Phone ?? customer.Phone;
        var dateOfBirth = dto.DateOfBirth ?? customer.DateOfBirth;
        var gender = dto.Gender ?? customer.Gender;
        var customerType = dto.CustomerType ?? customer.CustomerType;

        customer.Update(name, email, phone, address, dateOfBirth, gender, customerType);

        if (dto.IsActive.HasValue && dto.IsActive.Value != customer.IsActive)
        {
            if (dto.IsActive.Value)
                customer.Activate();
            else
                customer.Deactivate();
        }

        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(customer);
    }

    public async Task<bool> DeleteCustomerAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.FirstOrDefaultAsync(c => c.Id == id);
        if (customer == null) return false;

        customer.SoftDelete();
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private CustomerDto MapToDto(Customer customer)
    {
        return new CustomerDto
        {
            Id = customer.Id,
            Name = customer.Name,
            Email = customer.Email,
            Phone = customer.Phone,
            Address = customer.Address?.AddressLine ?? string.Empty,
            District = customer.Address?.District ?? string.Empty,
            City = customer.Address?.City ?? string.Empty,
            DateOfBirth = customer.DateOfBirth,
            Gender = customer.Gender,
            CustomerType = customer.CustomerType,
            IsActive = customer.IsActive,
            CreatedAt = customer.CreatedAt
        };
    }
}
