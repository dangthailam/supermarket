using Microsoft.EntityFrameworkCore;
using SuperMarket.Application.DTOs;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Entities;
using SuperMarket.Infrastructure.Data;

namespace SuperMarket.Infrastructure.Services;

public class TransactionService : ITransactionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly SuperMarketDbContext _context;

    public TransactionService(IUnitOfWork unitOfWork, SuperMarketDbContext context)
    {
        _unitOfWork = unitOfWork;
        _context = context;
    }

    public async Task<TransactionDto> CreateTransactionAsync(CreateTransactionDto dto)
    {
        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Create transaction using domain logic
            var transaction = Transaction.Create(
                dto.PaymentMethod,
                dto.DiscountAmount,
                dto.CustomerName,
                dto.CustomerPhone
            );

            // Process each item
            foreach (var itemDto in dto.Items)
            {
                // Fetch product
                var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {itemDto.ProductId} not found");

                // Check stock availability
                if (product.StockQuantity < itemDto.Quantity)
                    throw new Exception($"Insufficient stock for {product.Name}");

                // Add item to transaction (domain handles business logic)
                transaction.AddItem(product, itemDto.Quantity, itemDto.Discount);

                // Update product stock
                product.UpdateStock(-itemDto.Quantity);
                _unitOfWork.Products.Update(product);

                // Create inventory movement
                var inventoryMovement = new InventoryMovement(
                    product.Id,
                    MovementType.Sale,
                    -itemDto.Quantity,
                    transaction.TransactionNumber,
                    "Sale transaction"
                );
                await _unitOfWork.InventoryMovements.AddAsync(inventoryMovement);
            }

            // Calculate totals using domain logic
            transaction.CalculateTotals();

            // Save transaction
            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Set transaction ID for items and save
            foreach (var item in transaction.TransactionItems)
            {
                item.SetTransactionId(transaction.Id);
            }
            await _unitOfWork.TransactionItems.AddRangeAsync(transaction.TransactionItems);

            await _unitOfWork.CommitTransactionAsync();

            // Return the created transaction
            return await GetTransactionByIdAsync(transaction.Id)
                ?? throw new Exception("Failed to retrieve created transaction");
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    public async Task<TransactionDto?> GetTransactionByIdAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.TransactionItems)
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        return transaction == null ? null : MapToDto(transaction);
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Include(t => t.TransactionItems)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .OrderByDescending(t => t.TransactionDate)
            .AsNoTracking()
            .ToListAsync();

        return transactions.Select(MapToDto);
    }

    public async Task<IEnumerable<TransactionDto>> GetTodaysTransactionsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        return await GetTransactionsByDateRangeAsync(today, tomorrow);
    }

    public async Task<decimal> GetTodaysSalesAsync()
    {
        var today = DateTime.UtcNow.Date;
        var tomorrow = today.AddDays(1);

        var totalSales = await _context.Transactions
            .Where(t => t.TransactionDate >= today &&
                       t.TransactionDate < tomorrow &&
                       t.Status == TransactionStatus.Completed)
            .SumAsync(t => t.NetAmount);

        return totalSales;
    }

    public async Task<bool> CancelTransactionAsync(Guid id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.TransactionItems)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null)
            return false;

        // Check if transaction can be cancelled (domain logic)
        if (!transaction.CanBeCancelled())
            return false;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Get items that need stock restoration (domain logic)
            var itemsToRestore = transaction.GetItemsForStockRestoration();

            // Restore stock for each item
            foreach (var (productId, quantity) in itemsToRestore)
            {
                var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == productId);
                if (product != null)
                {
                    product.UpdateStock(quantity);
                    _unitOfWork.Products.Update(product);

                    // Create inventory movement
                    var inventoryMovement = new InventoryMovement(
                        product.Id,
                        MovementType.Return,
                        quantity,
                        transaction.TransactionNumber,
                        "Transaction cancelled"
                    );
                    await _unitOfWork.InventoryMovements.AddAsync(inventoryMovement);
                }
            }

            // Cancel transaction (domain logic)
            transaction.Cancel();
            _unitOfWork.Transactions.Update(transaction);

            await _unitOfWork.CommitTransactionAsync();
            return true;
        }
        catch
        {
            await _unitOfWork.RollbackTransactionAsync();
            throw;
        }
    }

    private static TransactionDto MapToDto(Transaction transaction)
    {
        return new TransactionDto
        {
            Id = transaction.Id,
            TransactionNumber = transaction.TransactionNumber,
            TransactionDate = transaction.TransactionDate,
            TotalAmount = transaction.TotalAmount,
            TaxAmount = transaction.TaxAmount,
            DiscountAmount = transaction.DiscountAmount,
            NetAmount = transaction.NetAmount,
            PaymentMethod = transaction.PaymentMethod,
            CustomerName = transaction.CustomerName,
            CustomerPhone = transaction.CustomerPhone,
            Status = transaction.Status,
            Items = transaction.TransactionItems.Select(i => new TransactionItemDto
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Discount = i.Discount,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
