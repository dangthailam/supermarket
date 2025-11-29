
using Microsoft.EntityFrameworkCore.Storage;
using SuperMarket.Application.Interfaces;
using SuperMarket.Domain.Entities;
using SuperMarket.Infrastructure.Data;

namespace SuperMarket.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly SuperMarketDbContext _context;
    private IDbContextTransaction? _transaction;

    public IRepository<Category> Categories { get; }
    public IRepository<Brand> Brands { get; }
    public IRepository<Product> Products { get; }
    public IRepository<Transaction> Transactions { get; }
    public IRepository<TransactionItem> TransactionItems { get; }
    public IRepository<InventoryMovement> InventoryMovements { get; }
    public IRepository<User> Users { get; }
    public IRepository<Provider> Providers { get; }
    public IRepository<Customer> Customers { get; }

    public UnitOfWork(SuperMarketDbContext context)
    {
        _context = context;
        Categories = new Repository<Category>(context);
        Products = new Repository<Product>(context);
        Transactions = new Repository<Transaction>(context);
        TransactionItems = new Repository<TransactionItem>(context);
        InventoryMovements = new Repository<InventoryMovement>(context);
        Users = new Repository<User>(context);
        Providers = new Repository<Provider>(context);
        Brands = new Repository<Brand>(context);
        Customers = new Repository<Customer>(context);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
            }
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
