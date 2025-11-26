using SuperMarket.Application.DTOs;

namespace SuperMarket.Application.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto);
    Task<TransactionDto?> GetTransactionByIdAsync(Guid id);
    Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TransactionDto>> GetTodaysTransactionsAsync();
    Task<decimal> GetTodaysSalesAsync();
    Task<bool> CancelTransactionAsync(Guid id);
}
