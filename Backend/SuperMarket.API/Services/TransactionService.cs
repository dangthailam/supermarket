using Microsoft.EntityFrameworkCore;
using SuperMarket.API.Data;
using SuperMarket.API.DTOs;
using SuperMarket.API.Interfaces;
using SuperMarket.API.Models;

namespace SuperMarket.API.Services;

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
            // Generate transaction number
            var transactionNumber = $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}";

            var transaction = new Transaction
            {
                TransactionNumber = transactionNumber,
                PaymentMethod = dto.PaymentMethod,
                CustomerName = dto.CustomerName,
                CustomerPhone = dto.CustomerPhone,
                DiscountAmount = dto.DiscountAmount,
                Status = TransactionStatus.Completed
            };

            decimal totalAmount = 0;
            var transactionItems = new List<TransactionItem>();

            // Process each item
            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(itemDto.ProductId);
                if (product == null)
                    throw new Exception($"Product with ID {itemDto.ProductId} not found");

                if (product.StockQuantity < itemDto.Quantity)
                    throw new Exception($"Insufficient stock for {product.Name}");

                var itemTotal = (product.Price * itemDto.Quantity) - itemDto.Discount;
                totalAmount += itemTotal;

                var transactionItem = new TransactionItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price,
                    Discount = itemDto.Discount,
                    TotalPrice = itemTotal
                };

                transactionItems.Add(transactionItem);

                // Update stock
                product.StockQuantity -= itemDto.Quantity;
                product.UpdatedAt = DateTime.UtcNow;
                _unitOfWork.Products.Update(product);

                // Create inventory movement
                var inventoryMovement = new InventoryMovement
                {
                    ProductId = product.Id,
                    Type = MovementType.Sale,
                    Quantity = -itemDto.Quantity,
                    Reference = transactionNumber,
                    Notes = "Sale transaction"
                };
                await _unitOfWork.InventoryMovements.AddAsync(inventoryMovement);
            }

            // Calculate tax (10% for example)
            var taxAmount = totalAmount * 0.10m;
            transaction.TotalAmount = totalAmount;
            transaction.TaxAmount = taxAmount;
            transaction.NetAmount = totalAmount + taxAmount - dto.DiscountAmount;

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync();

            // Add transaction items
            foreach (var item in transactionItems)
            {
                item.TransactionId = transaction.Id;
            }
            await _unitOfWork.TransactionItems.AddRangeAsync(transactionItems);

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

    public async Task<TransactionDto?> GetTransactionByIdAsync(int id)
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

    public async Task<bool> CancelTransactionAsync(int id)
    {
        var transaction = await _context.Transactions
            .Include(t => t.TransactionItems)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (transaction == null || transaction.Status == TransactionStatus.Cancelled)
            return false;

        await _unitOfWork.BeginTransactionAsync();

        try
        {
            // Restore stock for each item
            foreach (var item in transaction.TransactionItems)
            {
                var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                if (product != null)
                {
                    product.StockQuantity += item.Quantity;
                    product.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Products.Update(product);

                    // Create inventory movement
                    var inventoryMovement = new InventoryMovement
                    {
                        ProductId = product.Id,
                        Type = MovementType.Return,
                        Quantity = item.Quantity,
                        Reference = transaction.TransactionNumber,
                        Notes = "Transaction cancelled"
                    };
                    await _unitOfWork.InventoryMovements.AddAsync(inventoryMovement);
                }
            }

            transaction.Status = TransactionStatus.Cancelled;
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

    private TransactionDto MapToDto(Transaction transaction)
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
