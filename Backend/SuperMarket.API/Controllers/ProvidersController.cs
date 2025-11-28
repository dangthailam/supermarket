using Microsoft.AspNetCore.Mvc;
using SuperMarket.Application.DTOs;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Common;

namespace SuperMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProvidersController : ControllerBase
{
    private readonly IProviderService _providerService;

    public ProvidersController(IProviderService providerService)
    {
        _providerService = providerService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProviderDto>>> GetAllProviders()
    {
        var providers = await _providerService.GetAllProvidersAsync();
        return Ok(providers);
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<ProviderDto>>> GetProvidersPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        var paginationParams = new PaginationParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            SortBy = sortBy,
            SortDescending = sortDescending
        };

        var result = await _providerService.GetProvidersPagedAsync(paginationParams);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProviderDto>> GetProviderById(Guid id)
    {
        var provider = await _providerService.GetProviderByIdAsync(id);
        
        if (provider == null)
            return NotFound();
        
        return Ok(provider);
    }

    [HttpPost]
    public async Task<ActionResult<ProviderDto>> CreateProvider([FromBody] CreateProviderDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("Name is required");

        var provider = await _providerService.CreateProviderAsync(dto);
        return Ok(provider);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProviderDto>> UpdateProvider(Guid id, [FromBody] UpdateProviderDto dto)
    {
        var provider = await _providerService.UpdateProviderAsync(id, dto);
        
        if (provider == null)
            return NotFound();
        
        return Ok(provider);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteProvider(Guid id)
    {
        var success = await _providerService.DeleteProviderAsync(id);
        
        if (!success)
            return NotFound();
        
        return Ok();
    }
}
