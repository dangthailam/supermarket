using SuperMarket.Application.DTOs;
using SuperMarket.Domain.Common;

namespace SuperMarket.Application.Interfaces;

public interface IProviderService
{
    Task<ProviderDto> CreateProviderAsync(CreateProviderDto dto);
    Task<ProviderDto?> GetProviderByIdAsync(Guid id);
    Task<IEnumerable<ProviderDto>> GetAllProvidersAsync();
    Task<PaginatedResult<ProviderDto>> GetProvidersPagedAsync(PaginationParams paginationParams);
    Task<ProviderDto?> UpdateProviderAsync(Guid id, UpdateProviderDto dto);
    Task<bool> DeleteProviderAsync(Guid id);
}
