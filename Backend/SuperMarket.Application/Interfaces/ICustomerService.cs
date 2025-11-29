using SuperMarket.Application.DTOs;
using SuperMarket.Domain.Common;

namespace SuperMarket.Application.Interfaces;

public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto dto);
    Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
    Task<IEnumerable<CustomerDto>> GetAllCustomersAsync();
    Task<PaginatedResult<CustomerDto>> GetCustomersPagedAsync(PaginationParams paginationParams);
    Task<CustomerDto?> UpdateCustomerAsync(Guid id, UpdateCustomerDto dto);
    Task<bool> DeleteCustomerAsync(Guid id);
}
