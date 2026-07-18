using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.ConfirmEmail
{
    public record ConfirmEmailCommand(int UserId, string Token) : IRequest<Unit>;

}
