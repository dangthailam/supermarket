using SuperMarket.Domain.Entities;

namespace SuperMarket.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<TransactionItem> TransactionItems { get; }
    IRepository<InventoryMovement> InventoryMovements { get; }
    IRepository<Brand> Brands { get; }
    IRepository<User> Users { get; }
    IRepository<Provider> Providers { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
