using ECommerce.Application.Common;
using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Extensions;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Entities;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Payments.Commands.CreateCheckout;

public class CreateCheckoutCommandHandler : IRequestHandler<CreateCheckoutCommand, CheckoutResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<CreateCheckoutCommandHandler> _logger;
    private readonly IDistributedLockService _lockService;

    public CreateCheckoutCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<CreateCheckoutCommandHandler> logger,
        IDistributedLockService lockService)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
        _lockService = lockService;
    }

    public async Task<CheckoutResponseDto> Handle(CreateCheckoutCommand request, CancellationToken cancellationToken)
    {
        var lockKey = CacheKeys.CheckoutLock(request.UserId);
        var lockExpiry = TimeSpan.FromSeconds(30);
        var (acquired, lockValue) = await _lockService.TryAcquireAsync(lockKey, lockExpiry);

        if (!acquired)
            throw new AppException("A checkout is already in progress for your account. Please wait a moment and try again.");

        try
        {
            var cartItems = (await _unitOfWork.Carts.GetByUserIdAsync(request.UserId)).ToList();
            if (cartItems.Count == 0)
                throw new AppException("Cart is empty. Cannot place an order.");

            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(10));

            using var transaction = await _unitOfWork.BeginTransactionAsync(cts.Token);

            try
            {
                foreach (var item in cartItems)
                {
                    if (item.Product.Stock < item.Quantity)
                        throw new AppException(
                            $"Insufficient stock for '{item.Product.Name}'. Available: {item.Product.Stock}, Requested: {item.Quantity}");
                }

                var totalAmount = cartItems.Sum(i => i.Product.Price * i.Quantity);
                var existingOrder = await _unitOfWork.Orders.GetPendingOrderByUserIdAsync(request.UserId);
                Order order;
                Payment? existingPayment = null;

                if (existingOrder != null)
                {
                    await _unitOfWork.Products.RestoreStockForOrderAsync(existingOrder);
                    existingOrder.TotalPrice = totalAmount;
                    existingOrder.CreatedAt = DateTime.UtcNow;
                    existingOrder.OrderItems = cartItems.Select(ci => new OrderItem
                    {
                        ProductId = ci.ProductId,
                        Quantity = ci.Quantity,
                        Price = ci.Product.Price
                    }).ToList();
                    order = existingOrder;
                    _unitOfWork.Orders.Update(order);
                    existingPayment = await _unitOfWork.Payments.GetLatestByOrderIdAsync(order.Id);
                }
                else
                {
                    order = new Order
                    {
                        UserId = request.UserId,
                        Status = OrderStatus.Pending,
                        CreatedAt = DateTime.UtcNow,
                        TotalPrice = totalAmount,
                        OrderItems = cartItems.Select(ci => new OrderItem
                        {
                            ProductId = ci.ProductId,
                            Quantity = ci.Quantity,
                            Price = ci.Product.Price
                        }).ToList()
                    };
                    await _unitOfWork.Orders.AddAsync(order);
                }

                await _unitOfWork.SaveChangesAsync(cts.Token);

                foreach (var item in cartItems)
                {
                    var deducted = await _unitOfWork.Products.DeductStockAsync(item.ProductId, item.Quantity);
                    if (!deducted)
                        throw new AppException($"Sorry, '{item.Product.Name}' just went out of stock. Please update your cart.");
                }

                var hasReusablePendingIntent = existingPayment is not null && existingPayment.Status == PaymentStatus.Pending;

                var metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id.ToString() },
                    { "user_id", request.UserId.ToString() }
                };

                var intent = hasReusablePendingIntent
                    ? await _paymentService.UpdatePaymentIntentAsync(existingPayment!.PaymentIntentId, ToCents(totalAmount), cts.Token)
                    : await _paymentService.CreatePaymentIntentAsync(ToCents(totalAmount), "usd", metadata, cts.Token);

                order.PaymentIntentId = intent.Id;
                _unitOfWork.Orders.Update(order);

                if (hasReusablePendingIntent)
                {
                    existingPayment!.Amount = totalAmount;
                    existingPayment.ClientSecret = intent.ClientSecret;
                    existingPayment.PaymentIntentId = intent.Id;
                    existingPayment.UpdatedAt = DateTime.UtcNow;
                    _unitOfWork.Payments.Update(existingPayment);
                }
                else
                {
                    await _unitOfWork.Payments.AddAsync(new Payment
                    {
                        OrderId = order.Id,
                        PaymentIntentId = intent.Id,
                        ClientSecret = intent.ClientSecret,
                        Status = PaymentStatus.Pending,
                        Amount = totalAmount,
                        Currency = "usd"
                    });
                }

                await _unitOfWork.SaveChangesAsync(cts.Token);
                await transaction.CommitAsync(cts.Token);

                _logger.LogInformation(
                    "Checkout ready: Order {OrderId}, PaymentIntent {IntentId}, Amount {Amount}",
                    order.Id, intent.Id, totalAmount);

                return new CheckoutResponseDto
                {
                    OrderId = order.Id,
                    ClientSecret = intent.ClientSecret,
                    PublishableKey = _paymentService.GetPublishableKey(),
                    Amount = totalAmount,
                    Currency = "usd"
                };
            }
            catch (AppException)
            {
                await transaction.RollbackAsync(cts.Token);
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cts.Token);
                _logger.LogError(ex, "Checkout failed for user {UserId}", request.UserId);
                throw new AppException("An error occurred while processing your checkout. Please try again.");
            }
        }
        finally
        {
            await _lockService.ReleaseAsync(lockKey, lockValue);
        }
    }

    private static long ToCents(decimal amount) => (long)(amount * 100);
}