using ECommerce.Domain.Entities;

namespace ECommerce.Application.Interfaces.Repositories;

public interface IPaymentRepository : IRepository<Payment>
{
    Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId);
    Task<Payment?> GetLatestByOrderIdAsync(int orderId);
}