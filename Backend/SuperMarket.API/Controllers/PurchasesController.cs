using Microsoft.AspNetCore.Mvc;
using SuperMarket.Application.DTOs;
using SuperMarket.Application.Services;
using SuperMarket.Domain.Common;
using SuperMarket.Domain.Entities;

namespace SuperMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PurchasesController : ControllerBase
{
    private readonly IPurchaseService _purchaseService;
    private readonly ILogger<PurchasesController> _logger;

    public PurchasesController(IPurchaseService purchaseService, ILogger<PurchasesController> logger)
    {
        _purchaseService = purchaseService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetAll()
    {
        try
        {
            var purchases = await _purchaseService.GetAllPurchasesAsync();
            return Ok(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all purchases");
            return StatusCode(500, "An error occurred while retrieving purchases");
        }
    }

    [HttpGet("paged")]
    public async Task<ActionResult<PaginatedResult<PurchaseDto>>> GetPaged(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false)
    {
        try
        {
            var paginationParams = new PaginationParams
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                SortBy = sortBy,
                SortDescending = sortDescending
            };

            var result = await _purchaseService.GetPurchasesPagedAsync(paginationParams);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged purchases");
            return StatusCode(500, "An error occurred while retrieving purchases");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<PurchaseDto>> Get(Guid id)
    {
        try
        {
            var purchase = await _purchaseService.GetPurchaseByIdAsync(id);
            if (purchase == null)
                return NotFound($"Purchase with ID {id} not found");

            return Ok(purchase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchase {PurchaseId}", id);
            return StatusCode(500, "An error occurred while retrieving the purchase");
        }
    }

    [HttpGet("provider/{providerId}")]
    public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetByProvider(Guid providerId)
    {
        try
        {
            var purchases = await _purchaseService.GetPurchasesByProviderAsync(providerId);
            return Ok(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchases for provider {ProviderId}", providerId);
            return StatusCode(500, "An error occurred while retrieving purchases");
        }
    }

    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<PurchaseDto>>> GetByStatus(int status)
    {
        try
        {
            if (!Enum.IsDefined(typeof(PurchaseStatus), status))
                return BadRequest("Invalid status value");

            var purchases = await _purchaseService.GetPurchasesByStatusAsync((PurchaseStatus)status);
            return Ok(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting purchases with status {Status}", status);
            return StatusCode(500, "An error occurred while retrieving purchases");
        }
    }

    [HttpPost]
    public async Task<ActionResult<PurchaseDto>> Create([FromBody] CreatePurchaseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var purchase = await _purchaseService.CreatePurchaseAsync(dto);
            return CreatedAtAction(nameof(Get), new { id = purchase.Id }, purchase);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error creating purchase");
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating purchase");
            return StatusCode(500, "An error occurred while creating the purchase");
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<PurchaseDto>> Update(Guid id, [FromBody] UpdatePurchaseDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var purchase = await _purchaseService.UpdatePurchaseAsync(id, dto);
            if (purchase == null)
                return NotFound($"Purchase with ID {id} not found");

            return Ok(purchase);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Validation error updating purchase {PurchaseId}", id);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating purchase {PurchaseId}", id);
            return StatusCode(500, "An error occurred while updating the purchase");
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var result = await _purchaseService.DeletePurchaseAsync(id);
            if (!result)
                return NotFound($"Purchase with ID {id} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting purchase {PurchaseId}", id);
            return StatusCode(500, "An error occurred while deleting the purchase");
        }
    }
}
