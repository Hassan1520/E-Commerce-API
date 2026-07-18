using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByUserIdAsync(int userId);
    Task<Order?> GetByIdWithItemsAsync(int id , int userId);
    Task<Order?> GetByIdWithItemsAsync(int id);
    Task<IEnumerable<Order>> GetAllOrdersWithItemsAsync();
    Task<Order?> GetPendingOrderByUserIdAsync(int userId);
    Task<IEnumerable<Order>> GetStalePendingOrdersAsync(DateTime cutoffUtc);

}
