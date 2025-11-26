using Microsoft.AspNetCore.Mvc;
using SuperMarket.Application.Interfaces;
using SuperMarket.Application.DTOs;

namespace SuperMarket.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionsController(ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TransactionDto>> GetTransactionById(Guid id)
    {
        var transaction = await _transactionService.GetTransactionByIdAsync(id);
        if (transaction == null)
            return NotFound();

        return Ok(transaction);
    }

    [HttpGet("today")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTodaysTransactions()
    {
        var transactions = await _transactionService.GetTodaysTransactionsAsync();
        return Ok(transactions);
    }

    [HttpGet("today/sales")]
    public async Task<ActionResult<decimal>> GetTodaysSales()
    {
        var sales = await _transactionService.GetTodaysSalesAsync();
        return Ok(new { totalSales = sales });
    }

    [HttpGet("date-range")]
    public async Task<ActionResult<IEnumerable<TransactionDto>>> GetTransactionsByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        var transactions = await _transactionService.GetTransactionsByDateRangeAsync(startDate, endDate);
        return Ok(transactions);
    }

    [HttpPost]
    public async Task<ActionResult<TransactionDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        try
        {
            var transaction = await _transactionService.CreateTransactionAsync(dto);
            return CreatedAtAction(nameof(GetTransactionById), new { id = transaction.Id }, transaction);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult> CancelTransaction(Guid id)
    {
        try
        {
            var result = await _transactionService.CancelTransactionAsync(id);
            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
