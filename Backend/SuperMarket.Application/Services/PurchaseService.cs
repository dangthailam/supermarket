using SuperMarket.Application.DTOs;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Common;
using SuperMarket.Domain.Entities;

namespace SuperMarket.Application.Services;

public interface IPurchaseService
{
    Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseDto dto);
    Task<PurchaseDto?> GetPurchaseByIdAsync(Guid id);
    Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync();
    Task<PaginatedResult<PurchaseDto>> GetPurchasesPagedAsync(PaginationParams paginationParams);
    Task<PurchaseDto?> UpdatePurchaseAsync(Guid id, UpdatePurchaseDto dto);
    Task<bool> DeletePurchaseAsync(Guid id);
    Task<IEnumerable<PurchaseDto>> GetPurchasesByProviderAsync(Guid providerId);
    Task<IEnumerable<PurchaseDto>> GetPurchasesByStatusAsync(PurchaseStatus status);
}

public class PurchaseService : IPurchaseService
{
    private readonly IUnitOfWork _unitOfWork;

    public PurchaseService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PurchaseDto> CreatePurchaseAsync(CreatePurchaseDto dto)
    {
        // Validate provider exists
        var provider = await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == dto.ProviderId);
        if (provider == null)
            throw new ArgumentException($"Provider with ID {dto.ProviderId} not found.");

        // Validate products exist
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
        if (products.Count() != productIds.Count)
            throw new ArgumentException("One or more products not found.");

        // Generate unique purchase code
        var code = await GeneratePurchaseCodeAsync();

        // Create purchase items
        var purchaseItems = new List<PurchaseItem>();
        foreach (var itemDto in dto.Items)
        {
            var product = products.First(p => p.Id == itemDto.ProductId);
            var purchaseItem = new PurchaseItem(
                product,
                itemDto.Quantity,
                itemDto.PurchasePrice,
                itemDto.Discount,
                itemDto.Note
            );
            purchaseItems.Add(purchaseItem);
        }

        // Create purchase
        var purchase = new Purchase(
            purchaseItems,
            dto.PurchaseDate,
            code,
            provider,
            (PurchaseStatus)dto.Status,
            dto.Note
        );

        await _unitOfWork.Purchases.AddAsync(purchase);
        
        // Update inventory if status is Paid
        if (dto.Status == (int)PurchaseStatus.Paid)
        {
            foreach (var item in purchaseItems)
            {
                item.Product.UpdateStock(item.Quantity);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(purchase);
    }

    public async Task<PurchaseDto?> GetPurchaseByIdAsync(Guid id)
    {
        var purchases = await _unitOfWork.Purchases.FindAsync(p => p.Id == id);
        var purchase = purchases.FirstOrDefault();
        
        if (purchase == null) return null;

        return await MapToDtoAsync(purchase);
    }

    public async Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync()
    {
        var purchases = await _unitOfWork.Purchases.GetAllAsync();
        return await MapToDtosAsync(purchases);
    }

    public async Task<PaginatedResult<PurchaseDto>> GetPurchasesPagedAsync(PaginationParams paginationParams)
    {
        Func<IQueryable<Purchase>, IOrderedQueryable<Purchase>> orderBy = query =>
        {
            return paginationParams.SortBy?.ToLower() switch
            {
                "code" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.Code)
                    : query.OrderBy(p => p.Code),
                "purchasedate" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.PurchaseDate)
                    : query.OrderBy(p => p.PurchaseDate),
                "status" => paginationParams.SortDescending
                    ? query.OrderByDescending(p => p.Status)
                    : query.OrderBy(p => p.Status),
                _ => query.OrderByDescending(p => p.PurchaseDate)
            };
        };

        var paginatedResult = await _unitOfWork.Purchases.GetPagedAsync(
            paginationParams.PageNumber,
            paginationParams.PageSize,
            orderBy: orderBy
        );

        var dtos = await MapToDtosAsync(paginatedResult.Items);

        return new PaginatedResult<PurchaseDto>(
            dtos,
            paginatedResult.PageNumber,
            paginatedResult.PageSize,
            paginatedResult.TotalCount
        );
    }

    public async Task<PurchaseDto?> UpdatePurchaseAsync(Guid id, UpdatePurchaseDto dto)
    {
        var purchases = await _unitOfWork.Purchases.FindAsync(p => p.Id == id);
        var purchase = purchases.FirstOrDefault();
        
        if (purchase == null) return null;

        var oldStatus = purchase.Status;

        // Update purchase properties using reflection since properties are private
        if (dto.PurchaseDate.HasValue)
        {
            var purchaseDateProperty = typeof(Purchase).GetProperty("PurchaseDate");
            purchaseDateProperty?.SetValue(purchase, dto.PurchaseDate.Value);
        }

        if (dto.ProviderId.HasValue)
        {
            var provider = await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == dto.ProviderId.Value);
            if (provider == null)
                throw new ArgumentException($"Provider with ID {dto.ProviderId.Value} not found.");
            
            var providerIdProperty = typeof(Purchase).GetProperty("ProviderId");
            var providerProperty = typeof(Purchase).GetProperty("Provider");
            providerIdProperty?.SetValue(purchase, provider.Id);
            providerProperty?.SetValue(purchase, provider);
        }

        if (dto.Status.HasValue)
        {
            var statusProperty = typeof(Purchase).GetProperty("Status");
            statusProperty?.SetValue(purchase, (PurchaseStatus)dto.Status.Value);

            // Handle inventory changes when status changes
            if (oldStatus != PurchaseStatus.Paid && dto.Status.Value == (int)PurchaseStatus.Paid)
            {
                // Status changed to Paid - add stock
                foreach (var item in purchase.PurchaseItems)
                {
                    item.Product.UpdateStock(item.Quantity);
                }
            }
            else if (oldStatus == PurchaseStatus.Paid && dto.Status.Value != (int)PurchaseStatus.Paid)
            {
                // Status changed from Paid - remove stock
                foreach (var item in purchase.PurchaseItems)
                {
                    item.Product.UpdateStock(-item.Quantity);
                }
            }
        }

        if (dto.Note != null)
        {
            var noteProperty = typeof(Purchase).GetProperty("Note");
            noteProperty?.SetValue(purchase, dto.Note);
        }

        // Update items if provided
        if (dto.Items != null)
        {
            // Remove old items
            var oldItems = purchase.PurchaseItems.ToList();
            foreach (var item in oldItems)
            {
                _unitOfWork.PurchaseItems.Remove(item);
            }

            // Add new items
            var newItems = new List<PurchaseItem>();
            foreach (var itemDto in dto.Items)
            {
                var product = await _unitOfWork.Products.FirstOrDefaultAsync(p => p.Id == itemDto.ProductId);
                if (product == null)
                    throw new ArgumentException($"Product with ID {itemDto.ProductId} not found.");

                var newItem = new PurchaseItem(
                    product,
                    itemDto.Quantity,
                    itemDto.PurchasePrice,
                    itemDto.Discount,
                    itemDto.Note
                );
                newItems.Add(newItem);
            }

            var itemsProperty = typeof(Purchase).GetProperty("PurchaseItems");
            itemsProperty?.SetValue(purchase, newItems);
        }

        _unitOfWork.Purchases.Update(purchase);
        await _unitOfWork.SaveChangesAsync();

        return await MapToDtoAsync(purchase);
    }

    public async Task<bool> DeletePurchaseAsync(Guid id)
    {
        var purchases = await _unitOfWork.Purchases.FindAsync(p => p.Id == id);
        var purchase = purchases.FirstOrDefault();
        
        if (purchase == null) return false;

        // If purchase was paid, reverse the inventory changes
        if (purchase.Status == PurchaseStatus.Paid)
        {
            foreach (var item in purchase.PurchaseItems)
            {
                item.Product.UpdateStock(-item.Quantity);
            }
        }

        _unitOfWork.Purchases.Remove(purchase);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<PurchaseDto>> GetPurchasesByProviderAsync(Guid providerId)
    {
        var purchases = await _unitOfWork.Purchases.FindAsync(p => p.ProviderId == providerId);
        return await MapToDtosAsync(purchases);
    }

    public async Task<IEnumerable<PurchaseDto>> GetPurchasesByStatusAsync(PurchaseStatus status)
    {
        var purchases = await _unitOfWork.Purchases.FindAsync(p => p.Status == status);
        return await MapToDtosAsync(purchases);
    }

    private async Task<string> GeneratePurchaseCodeAsync()
    {
        var today = DateTime.UtcNow;
        var prefix = $"PO{today:yyyyMMdd}";
        
        var allPurchases = await _unitOfWork.Purchases.GetAllAsync();
        var todayPurchases = allPurchases.Where(p => p.Code.StartsWith(prefix)).ToList();
        
        var maxNumber = 0;
        foreach (var purchase in todayPurchases)
        {
            if (purchase.Code.Length > prefix.Length)
            {
                var numberPart = purchase.Code.Substring(prefix.Length);
                if (int.TryParse(numberPart, out var number))
                {
                    maxNumber = Math.Max(maxNumber, number);
                }
            }
        }

        return $"{prefix}{(maxNumber + 1):D4}";
    }

    private async Task<PurchaseDto> MapToDtoAsync(Purchase purchase)
    {
        var totalAmount = purchase.PurchaseItems.Sum(i => 
            i.Quantity * i.PurchasePrice - (i.Discount ?? 0));

        return new PurchaseDto
        {
            Id = purchase.Id,
            Code = purchase.Code,
            PurchaseDate = purchase.PurchaseDate,
            ProviderId = purchase.ProviderId,
            ProviderName = purchase.Provider.Name,
            ProviderCode = purchase.Provider.Code,
            Status = (int)purchase.Status,
            StatusText = purchase.Status.ToString(),
            Note = purchase.Note,
            TotalAmount = totalAmount,
            Items = purchase.PurchaseItems.Select(i => new PurchaseItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ProductSku = i.Product.SKU,
                Quantity = i.Quantity,
                PurchasePrice = i.PurchasePrice,
                Discount = i.Discount,
                TotalPrice = i.Quantity * i.PurchasePrice - (i.Discount ?? 0),
                Note = i.Note
            }).ToList()
        };
    }

    private async Task<IEnumerable<PurchaseDto>> MapToDtosAsync(IEnumerable<Purchase> purchases)
    {
        var dtos = new List<PurchaseDto>();
        foreach (var purchase in purchases)
        {
            dtos.Add(await MapToDtoAsync(purchase));
        }
        return dtos;
    }
}
