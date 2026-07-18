using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Domain.Entities;

namespace ECommerce.Application.Common.Extensions;

public static class ProductRepositoryExtensions
{
    /// <summary>
    /// Restores (adds back) stock for every item in the given order.
    /// Used when an order is cancelled, a pending checkout is being replaced,
    /// or a payment fails after stock was already deducted.
    /// </summary>
    public static async Task RestoreStockForOrderAsync(this IProductRepository productRepository, Order order)
    {
        foreach (var item in order.OrderItems)
        {
            await productRepository.RestoreStockAsync(item.ProductId, item.Quantity);
        }
    }
}