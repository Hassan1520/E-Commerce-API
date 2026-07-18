using ECommerce.Infrastructure.Data;
using ECommerce.Domain.Entities;
using ECommerce.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Infrastructure.Repositories;

public class PaymentRepository : Repository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Payment?> GetByPaymentIntentIdAsync(string paymentIntentId) =>
        await _dbSet.FirstOrDefaultAsync(p => p.PaymentIntentId == paymentIntentId);

    public async Task<Payment?> GetLatestByOrderIdAsync(int orderId) =>
        await _dbSet
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
}