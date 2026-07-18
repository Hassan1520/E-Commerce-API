using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.ConfirmEmail
{
    public class ConfirmEmailCommandHandler(IAuthIdentityService authService) : IRequestHandler<ConfirmEmailCommand, Unit>
    {
        public async Task<Unit> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            await authService.ConfirmEmailAsync(request.UserId, request.Token);
            return Unit.Value;
        }
    }
}
