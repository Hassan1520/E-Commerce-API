using ECommerce.Application.DTOs.Payments;
using MediatR;

namespace ECommerce.Application.Features.Payments.Commands.CreateCheckout;

public record CreateCheckoutCommand(int UserId) : IRequest<CheckoutResponseDto>;