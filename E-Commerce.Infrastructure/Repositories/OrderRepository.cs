using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using ECommerce.Infrastructure.Data;
using ECommerce.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class OrderRepository : Repository<Order>, IOrderRepository
{
    public OrderRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(int userId) =>
        await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

    public async Task<Order?> GetByIdWithItemsAsync(int id , int userId) =>
        await _dbSet
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserId == userId)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetByIdWithItemsAsync(int id) =>
        await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync() =>
        await _dbSet
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    public async Task<Order?> GetPendingOrderByUserIdAsync(int userId)
    {
        return await _dbSet
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending);
    }
    public async Task<IEnumerable<Order>> GetStalePendingOrdersAsync(DateTime cutoffUtc) =>
        await _dbSet
            .Include(o => o.OrderItems)
            .Where(o => o.Status == OrderStatus.Pending && o.CreatedAt < cutoffUtc)
            .ToListAsync();
}
