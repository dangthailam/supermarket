using SuperMarket.API.DTOs;

namespace SuperMarket.API.Interfaces;

public interface ITransactionService
{
    Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto);
    Task<TransactionDto?> GetTransactionByIdAsync(int id);
    Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TransactionDto>> GetTodaysTransactionsAsync();
    Task<decimal> GetTodaysSalesAsync();
    Task<bool> CancelTransactionAsync(int id);
}
