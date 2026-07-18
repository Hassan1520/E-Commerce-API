using ECommerce.Domain.Entities;
using ECommerce.Infrastructure.Settings;
using ECommerce.Domain.Enums;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using Microsoft.Extensions.Options;
using Stripe;

using Microsoft.Extensions.Logging;

namespace ECommerce.Infrastructure.Services;

public class OrderCleanupService : IOrderCleanupService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly StripeSettings _stripeSettings;
    private readonly ILogger<OrderCleanupService> _logger;

    private static readonly TimeSpan AbandonedThreshold = TimeSpan.FromMinutes(30);

    public OrderCleanupService(
        IUnitOfWork unitOfWork,
        IOptions<StripeSettings> stripeSettings,
        ILogger<OrderCleanupService> logger)
    {
        _unitOfWork = unitOfWork;
        _stripeSettings = stripeSettings.Value;
        _logger = logger;

        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public async Task ReleaseAbandonedOrdersAsync()
    {
        var cutoff = DateTime.UtcNow.Subtract(AbandonedThreshold);
        var staleOrders = (await _unitOfWork.Orders.GetStalePendingOrdersAsync(cutoff)).ToList();

        if (staleOrders.Count == 0)
        {
            _logger.LogInformation("Order cleanup: no abandoned orders found.");
            return;
        }

        _logger.LogInformation("Order cleanup: found {Count} abandoned orders to release.", staleOrders.Count);

        foreach (var order in staleOrders)
        {
            try
            {
                foreach (var item in order.OrderItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId);
                    if (product is not null)
                    {
                        product.Stock += item.Quantity;
                        _unitOfWork.Products.Update(product);
                    }
                }
                await _unitOfWork.SaveChangesAsync();

                if (!string.IsNullOrEmpty(order.PaymentIntentId))
                    await TryCancelStripeIntentAsync(order.PaymentIntentId);

                order.Status = OrderStatus.Cancelled;
                _unitOfWork.Orders.Update(order);
                await _unitOfWork.SaveChangesAsync();

                var payment = await _unitOfWork.Payments.GetLatestByOrderIdAsync(order.Id);
                if (payment is not null && payment.Status == PaymentStatus.Pending)
                {
                    payment.Status = PaymentStatus.Failed;
                    payment.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Payments.Update(payment);
                    await _unitOfWork.SaveChangesAsync();
                }

                _logger.LogInformation(
                    "Order cleanup: released Order {OrderId} (created {CreatedAt}).",
                    order.Id, order.CreatedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order cleanup: failed to release Order {OrderId}.", order.Id);
            }
        }
    }

    private async Task TryCancelStripeIntentAsync(string paymentIntentId)
    {
        try
        {
            var service = new PaymentIntentService();
            await service.CancelAsync(paymentIntentId);
        }
        catch (StripeException ex)
        {
            _logger.LogInformation(
                "Could not cancel PaymentIntent {IntentId} (likely already in a terminal state): {Reason}",
                paymentIntentId, ex.StripeError?.Message);
        }
    }
}