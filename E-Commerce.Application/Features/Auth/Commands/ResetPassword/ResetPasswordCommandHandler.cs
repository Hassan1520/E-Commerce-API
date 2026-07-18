using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.ResetPassword
{
    public class ResetPasswordCommandHandler(IAuthIdentityService authService) : IRequestHandler<ResetPasswordCommand, Unit>
    {
        public async Task<Unit> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            await authService.ResetPasswordAsync(request.Dto);
            return Unit.Value;
        }
    }
}
