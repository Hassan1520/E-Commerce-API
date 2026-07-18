using ECommerce.Application.DTOs.Orders;
using ECommerce.Application.Features.Orders.Commands.UpdateOrder;
using ECommerce.Domain.Enums;
using FluentValidation;

namespace ECommerce.Application.Validators.Orders;

public class UpdateOrderStatusValidator : AbstractValidator<UpdateOrderStatusCommand>
{
    // Admin ÈÓ íÞÏÑ íÛíÑ áÜ Shipped Ãæ Delivered
    private static readonly string[] AllowedStatuses =
    [
        nameof(OrderStatus.Shipped),
        nameof(OrderStatus.Delivered)
    ];

    public UpdateOrderStatusValidator()
    {
        RuleFor(x => x.Dto.Status)
            .NotEmpty().WithMessage("Status is required.")
            .Must(s => AllowedStatuses.Contains(s, StringComparer.OrdinalIgnoreCase))
            .WithMessage($"Status must be one of: {string.Join(", ", AllowedStatuses)}");
    }
}