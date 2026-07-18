using ECommerce.Application.Common.Exceptions;
using ECommerce.Application.Common.Extensions;
using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Repositories;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ECommerce.Application.Features.Payments.Commands.HandleStripeWebhook;

public class HandleStripeWebhookCommandHandler : IRequestHandler<HandleStripeWebhookCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPaymentService _paymentService;
    private readonly ILogger<HandleStripeWebhookCommandHandler> _logger;

    public HandleStripeWebhookCommandHandler(
        IUnitOfWork unitOfWork,
        IPaymentService paymentService,
        ILogger<HandleStripeWebhookCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _paymentService = paymentService;
        _logger = logger;
    }

    public async Task<Unit> Handle(HandleStripeWebhookCommand request, CancellationToken cancellationToken)
    {
        WebhookEventResultDto webhookEvent;
        try
        {
            webhookEvent = _paymentService.ConstructWebhookEvent(request.Payload, request.StripeSignature);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Invalid Stripe webhook signature: {Message}", ex.Message);
            throw new AppException("Invalid webhook signature.", 400);
        }

        _logger.LogInformation("Stripe webhook received: {EventType}", webhookEvent.Type);

        if (string.IsNullOrEmpty(webhookEvent.PaymentIntentId))
            return Unit.Value;

        switch (webhookEvent.Type)
        {
            case "payment_intent.succeeded":
                await HandlePaymentSucceededAsync(webhookEvent.PaymentIntentId, cancellationToken);
                break;
            case "payment_intent.payment_failed":
                await HandlePaymentFailedAsync(webhookEvent.PaymentIntentId, cancellationToken);
                break;
        }

        return Unit.Value;
    }

    private async Task HandlePaymentSucceededAsync(string paymentIntentId, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Payments.GetByPaymentIntentIdAsync(paymentIntentId);
        if (payment is null || payment.Status == PaymentStatus.Succeeded) return;

        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(payment.OrderId);
        if (order is null) return;

        payment.Status = PaymentStatus.Succeeded;
        payment.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Payments.Update(payment);
        order.Status = OrderStatus.Confirmed;
        _unitOfWork.Orders.Update(order);
        await _unitOfWork.Carts.ClearUserCartAsync(order.UserId);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment succeeded for Order {OrderId}", order.Id);
    }

    private async Task HandlePaymentFailedAsync(string paymentIntentId, CancellationToken cancellationToken)
    {
        var payment = await _unitOfWork.Payments.GetByPaymentIntentIdAsync(paymentIntentId);
        if (payment is null || payment.Status == PaymentStatus.Failed) return;

        payment.Status = PaymentStatus.Failed;
        payment.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Payments.Update(payment);

        var order = await _unitOfWork.Orders.GetByIdWithItemsAsync(payment.OrderId);
        if (order is not null && order.Status == OrderStatus.Pending)
        {
            order.Status = OrderStatus.Cancelled;
            _unitOfWork.Orders.Update(order);
            await _unitOfWork.Products.RestoreStockForOrderAsync(order);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

}