using ECommerce.Application.DTOs.Payments;
using ECommerce.Application.Interfaces.Services;
using ECommerce.Infrastructure.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;

namespace ECommerce.Infrastructure.Services;

public class StripePaymentService : IPaymentService
{
    private readonly StripeSettings _settings;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(IOptions<StripeSettings> settings, ILogger<StripePaymentService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
        StripeConfiguration.ApiKey = _settings.SecretKey;
    }

    public async Task<PaymentIntentResultDto> CreatePaymentIntentAsync(
        long amountInCents,
        string currency,
        IDictionary<string, string> metadata,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();

        var intent = await service.CreateAsync(new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = currency,
            Metadata = metadata as Dictionary<string, string> ?? new Dictionary<string, string>(metadata)
        }, cancellationToken: cancellationToken);

        return new PaymentIntentResultDto
        {
            Id = intent.Id,
            ClientSecret = intent.ClientSecret
        };
    }

    public async Task<PaymentIntentResultDto> UpdatePaymentIntentAsync(
        string paymentIntentId,
        long amountInCents,
        CancellationToken cancellationToken = default)
    {
        var service = new PaymentIntentService();

        try
        {
            var intent = await service.UpdateAsync(
                paymentIntentId,
                new PaymentIntentUpdateOptions { Amount = amountInCents },
                cancellationToken: cancellationToken);

            return new PaymentIntentResultDto
            {
                Id = intent.Id,
                ClientSecret = intent.ClientSecret
            };
        }
        catch (StripeException ex)
        {
            _logger.LogWarning(ex, "Could not update PaymentIntent {IntentId}, creating a new one.", paymentIntentId);

            var intent = await service.CreateAsync(
                new PaymentIntentCreateOptions { Amount = amountInCents, Currency = "usd" },
                cancellationToken: cancellationToken);

            return new PaymentIntentResultDto
            {
                Id = intent.Id,
                ClientSecret = intent.ClientSecret
            };
        }
    }

    public string GetPublishableKey() => _settings.PublishableKey;

    public WebhookEventResultDto ConstructWebhookEvent(string payload, string signatureHeader)
    {
        var stripeEvent = EventUtility.ConstructEvent(payload, signatureHeader, _settings.WebhookSecret);

        string? paymentIntentId = stripeEvent.Data.Object is PaymentIntent intent
            ? intent.Id
            : null;

        return new WebhookEventResultDto
        {
            Type = stripeEvent.Type,
            PaymentIntentId = paymentIntentId
        };
    }
}