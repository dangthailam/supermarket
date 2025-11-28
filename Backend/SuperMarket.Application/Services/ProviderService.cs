using SuperMarket.Application.DTOs;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Common;
using SuperMarket.Domain.Entities;
using SuperMarket.Domain.ValueObjects;

namespace SuperMarket.Application.Services;

public class ProviderService : IProviderService
{
    private readonly IUnitOfWork _unitOfWork;

    public ProviderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ProviderDto> CreateProviderAsync(CreateProviderDto dto)
    {
        // Generate code if not provided
        var code = GenerateProviderCode(dto.Name);

        // Create address
        var address = new Address(
            dto.Address ?? "Not specified",
            dto.District ?? "Not specified",
            dto.City ?? "Not specified"
        );

        var provider = new Provider(
            dto.Name,
            code,
            dto.Phone,
            dto.Email,
            address,
            dto.Note ?? string.Empty,
            dto.CompanyName ?? string.Empty,
            dto.TaxNumber ?? string.Empty
        );

        await _unitOfWork.Providers.AddAsync(provider);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(provider, dto.ContactName);
    }

    public async Task<ProviderDto?> GetProviderByIdAsync(Guid id)
    {
        var provider = await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider == null) return null;

        return MapToDto(provider);
    }

    public async Task<IEnumerable<ProviderDto>> GetAllProvidersAsync()
    {
        var providers = await _unitOfWork.Providers.GetAllAsync();
        return providers.Select(p => MapToDto(p)).ToList();
    }

    public async Task<PaginatedResult<ProviderDto>> GetProvidersPagedAsync(PaginationParams paginationParams)
    {
        var allProviders = await _unitOfWork.Providers.GetAllAsync();
        var filtered = allProviders.AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(paginationParams.SearchTerm))
        {
            var searchTerm = paginationParams.SearchTerm.ToLower();
            filtered = filtered.Where(p =>
                p.Name.ToLower().Contains(searchTerm) ||
                p.Code.ToLower().Contains(searchTerm) ||
                p.Phone.ToLower().Contains(searchTerm) ||
                p.Email.ToLower().Contains(searchTerm)
            );
        }

        // Apply sorting
        IOrderedQueryable<Provider> sorted = paginationParams.SortBy?.ToLower() switch
        {
            "name" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.Name)
                : filtered.OrderBy(p => p.Name),
            "code" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.Code)
                : filtered.OrderBy(p => p.Code),
            "phone" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.Phone)
                : filtered.OrderBy(p => p.Phone),
            "createdat" => paginationParams.SortDescending
                ? filtered.OrderByDescending(p => p.CreatedAt)
                : filtered.OrderBy(p => p.CreatedAt),
            _ => filtered.OrderBy(p => p.Name)
        };

        var count = sorted.Count();
        var providers = sorted
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            .Take(paginationParams.PageSize)
            .ToList();

        var providerDtos = providers.Select(p => MapToDto(p)).ToList();

        return new PaginatedResult<ProviderDto>(providerDtos, count, paginationParams.PageNumber, paginationParams.PageSize);
    }

    public async Task<ProviderDto?> UpdateProviderAsync(Guid id, UpdateProviderDto dto)
    {
        var provider = await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider == null) return null;

        // Update properties
        var name = dto.Name ?? provider.Name;
        var code = dto.Code ?? provider.Code;
        var phone = dto.Phone ?? provider.Phone;
        var email = dto.Email ?? provider.Email;
        var note = dto.Note ?? provider.Note;
        var companyName = dto.CompanyName ?? provider.CompanyName;
        var taxNumber = dto.TaxNumber ?? provider.TaxNumber;

        // Update address if provided
        var address = provider.Address;
        if (!string.IsNullOrWhiteSpace(dto.Address) || !string.IsNullOrWhiteSpace(dto.District) || !string.IsNullOrWhiteSpace(dto.City))
        {
            address = new Address(
                dto.Address ?? provider.Address.AddressLine,
                dto.District ?? provider.Address.District,
                dto.City ?? provider.Address.City
            );
        }

        // Create new provider with updated values (since properties are private set)
        var updatedProvider = new Provider(name, code, phone, email, address, note, companyName, taxNumber);
        
        // Copy the Id from the original to maintain database reference
        var idProperty = typeof(Provider).BaseType?.GetProperty("Id");
        idProperty?.SetValue(updatedProvider, id);

        _unitOfWork.Providers.Update(updatedProvider);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(updatedProvider, dto.Name);
    }

    public async Task<bool> DeleteProviderAsync(Guid id)
    {
        var provider = await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == id);
        if (provider == null) return false;

        _unitOfWork.Providers.Remove(provider);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private ProviderDto MapToDto(Provider provider, string? contactName = null)
    {
        return new ProviderDto
        {
            Id = provider.Id,
            Name = provider.Name,
            Code = provider.Code,
            Phone = provider.Phone,
            Email = provider.Email,
            Address = provider.Address.AddressLine,
            District = provider.Address.District,
            City = provider.Address.City,
            Note = provider.Note,
            CompanyName = provider.CompanyName,
            TaxNumber = provider.TaxNumber,
            ContactName = contactName ?? string.Empty
        };
    }

    private string GenerateProviderCode(string providerName)
    {
        // Generate code from provider name: take first 3 letters + timestamp
        var prefix = new string(providerName.Where(char.IsLetterOrDigit).Take(3).ToArray()).ToUpper();
        if (prefix.Length < 3) prefix = prefix.PadRight(3, 'X');
        
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString().Substring(5);
        return $"{prefix}-{timestamp}";
    }
}
