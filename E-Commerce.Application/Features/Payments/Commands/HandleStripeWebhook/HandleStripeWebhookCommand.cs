using MediatR;

namespace ECommerce.Application.Features.Payments.Commands.HandleStripeWebhook;

public record HandleStripeWebhookCommand(string Payload, string StripeSignature) : IRequest<Unit>;