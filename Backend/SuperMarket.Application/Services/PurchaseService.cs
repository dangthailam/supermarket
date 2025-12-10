using Microsoft.EntityFrameworkCore;
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
        var provider = await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == dto.ProviderId)
            ?? throw new ArgumentException($"Provider with ID {dto.ProviderId} not found.");

        // Validate products exist
        var productIds = dto.Items.Select(i => i.ProductId).ToList();
        var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));
        if (products.Count() != productIds.Count)
            throw new ArgumentException("One or more products not found.");

        // Generate unique purchase code
        var allPurchases = await _unitOfWork.Purchases.GetAllAsync();
        var code = PurchaseCodeGenerator.GenerateCode(allPurchases);

        // Create purchase items
        var purchaseItems = dto.Items
            .Select(itemDto =>
            {
                var product = products.First(p => p.Id == itemDto.ProductId);
                return new PurchaseItem(
                    product,
                    itemDto.Quantity,
                    itemDto.PurchasePrice,
                    itemDto.Discount,
                    itemDto.Note
                );
            })
            .ToList();

        // Create purchase (initial status is Pending)
        var purchase = new Purchase(
            purchaseItems,
            dto.PurchaseDate,
            code,
            provider,
            PurchaseStatus.Pending,
            dto.Note
        );

        // If incoming status is Paid, transition to Paid (handles inventory updates)
        if (dto.Status == (int)PurchaseStatus.Paid)
        {
            purchase.MarkAsPaid();
        }

        await _unitOfWork.PurchaseItems.AddRangeAsync(purchaseItems);
        await _unitOfWork.Purchases.AddAsync(purchase);
        await _unitOfWork.SaveChangesAsync();

        purchase = await _unitOfWork.Set<Purchase>()
            .Include(p => p.Provider)
            .Include(p => p.PurchaseItems)
            .ThenInclude(pi => pi.Product)
            .FirstAsync(p => p.Id == purchase.Id);

        return await MapToDtoAsync(purchase);
    }

    public async Task<PurchaseDto?> GetPurchaseByIdAsync(Guid id)
    {
        var purchase = await _unitOfWork.Set<Purchase>()
            .Include(p => p.Provider)
            .Include(p => p.PurchaseItems)
            .ThenInclude(pi => pi.Product)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return null;

        return await MapToDtoAsync(purchase);
    }

    public async Task<IEnumerable<PurchaseDto>> GetAllPurchasesAsync()
    {
        var purchases = await _unitOfWork.Set<Purchase>()
            .Include(p => p.Provider)
            .Include(p => p.PurchaseItems)
            .ThenInclude(pi => pi.Product)
            .AsNoTracking()
            .ToListAsync();

        return await MapToDtosAsync(purchases);
    }

    public async Task<PaginatedResult<PurchaseDto>> GetPurchasesPagedAsync(PaginationParams paginationParams)
    {
        var paginatedResult = await _unitOfWork.Purchases.GetPagedAsync(
            paginationParams.PageNumber,
            paginationParams.PageSize,
            orderBy: CreateSortExpression(paginationParams)
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
        var purchase = await _unitOfWork.Set<Purchase>()
            .Include(p => p.PurchaseItems)
            .FirstOrDefaultAsync(p => p.Id == id) ?? throw new ArgumentException($"Purchase with ID {id} not found.");

        // Update purchase metadata
        if (dto.PurchaseDate.HasValue || dto.ProviderId.HasValue || dto.Note != null || dto.Items != null)
        {
            var provider = dto.ProviderId.HasValue
                ? await _unitOfWork.Providers.FirstOrDefaultAsync(p => p.Id == dto.ProviderId.Value)
                    ?? throw new ArgumentException($"Provider with ID {dto.ProviderId.Value} not found.")
                : purchase.Provider;

            var purchaseDate = dto.PurchaseDate ?? purchase.PurchaseDate;
            var note = dto.Note ?? purchase.Note;
            var items = dto.Items != null
                ? await CreatePurchaseItemsAsync(dto.Items)
                : purchase.PurchaseItems;

            _unitOfWork.PurchaseItems.RemoveRange(purchase.PurchaseItems);

            purchase.UpdatePurchase(purchaseDate, provider, note, items);
        }

        // Handle status transition
        if (dto.Status.HasValue)
        {
            var newStatus = (PurchaseStatus)dto.Status.Value;
            purchase.TransitionToStatus(newStatus);
        }

        _unitOfWork.Purchases.Update(purchase);
        await _unitOfWork.SaveChangesAsync();

        purchase = await _unitOfWork.Set<Purchase>()
            .Include(p => p.PurchaseItems)
            .ThenInclude(pi => pi.Product)
            .FirstAsync(p => p.Id == id);

        return await MapToDtoAsync(purchase);
    }

    public async Task<bool> DeletePurchaseAsync(Guid id)
    {
        var purchase = await _unitOfWork.Set<Purchase>()
        .Include(p => p.PurchaseItems)
        .ThenInclude(pi => pi.Product)
        .FirstOrDefaultAsync(p => p.Id == id);

        if (purchase == null) return false;

        // Cancel the purchase (handles inventory reversal if needed)
        purchase.Cancel();

        foreach (var product in purchase.PurchaseItems
            .Select(pi => pi.Product).ToList())
        {
            _unitOfWork.Products.Update(product);
        }
        _unitOfWork.PurchaseItems.RemoveRange(purchase.PurchaseItems);
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

    /// <summary>
    /// Creates a sort expression based on pagination parameters.
    /// </summary>
    private static Func<IQueryable<Purchase>, IOrderedQueryable<Purchase>> CreateSortExpression(
        PaginationParams paginationParams)
    {
        return query => paginationParams.SortBy?.ToLower() switch
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
    }

    /// <summary>
    /// Creates PurchaseItem entities from DTOs with product validation.
    /// </summary>
    private async Task<ICollection<PurchaseItem>> CreatePurchaseItemsAsync(
        IEnumerable<CreatePurchaseItemDto> itemDtos)
    {
        var items = new List<PurchaseItem>();
        var productIds = itemDtos.Select(i => i.ProductId).Distinct().ToList();
        var products = await _unitOfWork.Products.FindAsync(p => productIds.Contains(p.Id));

        foreach (var itemDto in itemDtos)
        {
            var product = products.FirstOrDefault(p => p.Id == itemDto.ProductId)
                ?? throw new ArgumentException($"Product with ID {itemDto.ProductId} not found.");

            items.Add(new PurchaseItem(
                product,
                itemDto.Quantity,
                itemDto.PurchasePrice,
                itemDto.Discount,
                itemDto.Note
            ));
        }

        await _unitOfWork.PurchaseItems.AddRangeAsync(items);

        return items;
    }

    /// <summary>
    /// Maps a single Purchase to its DTO representation.
    /// </summary>
    private static Task<PurchaseDto> MapToDtoAsync(Purchase purchase)
    {
        var totalAmount = purchase.PurchaseItems.Sum(i =>
            i.Quantity * i.PurchasePrice - (i.Discount ?? 0));

        var dto = new PurchaseDto
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

        return Task.FromResult(dto);
    }

    /// <summary>
    /// Maps multiple Purchase entities to their DTO representations.
    /// </summary>
    private static async Task<IEnumerable<PurchaseDto>> MapToDtosAsync(IEnumerable<Purchase> purchases)
    {
        var dtos = new List<PurchaseDto>();
        foreach (var purchase in purchases)
        {
            dtos.Add(await MapToDtoAsync(purchase));
        }
        return dtos;
    }
}
