using ECommerce.Application.DTOs.Cart;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface ICartRepository : IRepository<CartItem>
{
    Task<IEnumerable<CartItem>> GetByUserIdAsync(int userId);
    Task<CartItem?> GetByUserAndProductAsync(int userId, int productId);
    Task ClearUserCartAsync(int userId);
}
