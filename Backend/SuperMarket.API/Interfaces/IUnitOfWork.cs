using SuperMarket.API.Models;

namespace SuperMarket.API.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Category> Categories { get; }
    IRepository<Product> Products { get; }
    IRepository<Transaction> Transactions { get; }
    IRepository<TransactionItem> TransactionItems { get; }
    IRepository<InventoryMovement> InventoryMovements { get; }
    IRepository<User> Users { get; }

    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
