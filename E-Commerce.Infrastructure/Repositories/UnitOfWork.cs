using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace ECommerce.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(
        ApplicationDbContext context,
        IProductRepository products,
        ICategoryRepository categories,
        ICartRepository carts,
        IOrderRepository orders,
        IPaymentRepository payments,
        IRefreshTokenRepository refreshTokens,
        IRepository<ProductImage> productImages)
    {
        _context = context;
        Products = products;
        Categories = categories;
        Carts = carts;
        Orders = orders;
        Payments = payments;
        RefreshTokens = refreshTokens;
        ProductImages = productImages;
    }

    public IProductRepository Products { get; }
    public ICategoryRepository Categories { get; }
    public ICartRepository Carts { get; }
    public IOrderRepository Orders { get; }
    public IPaymentRepository Payments { get; }
    public IRefreshTokenRepository RefreshTokens { get; }
    public IRepository<ProductImage> ProductImages { get; }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        await _context.SaveChangesAsync(cancellationToken);

    public int SaveChanges() =>
        _context.SaveChanges();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        _context.Database.BeginTransactionAsync(cancellationToken);
}
