using ECommerce.Infrastructure.Data;
using ECommerce.Domain.Entities;
using ECommerce.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class CartRepository : Repository<CartItem>, ICartRepository
{
    public CartRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId) =>
        await _dbSet
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

    public async Task<CartItem?> GetByUserAndProductAsync(int userId, int productId) =>
        await _dbSet.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

    public async Task ClearUserCartAsync(int userId)
    {
        var items = await _dbSet.Where(c => c.UserId == userId).ToListAsync();
        _dbSet.RemoveRange(items);
    }
}
