using ECommerce.Application.DTOs.Payments;

namespace ECommerce.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        long amountInCents,
        string currency,
        IDictionary<string, string> metadata,
        CancellationToken cancellationToken = default);

    Task<PaymentIntentResultDto> UpdatePaymentIntentAsync(
        string paymentIntentId,
        long amountInCents,
        CancellationToken cancellationToken = default);

    string GetPublishableKey();

    WebhookEventResultDto ConstructWebhookEvent(string payload, string signatureHeader);
}