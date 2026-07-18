using ECommerce.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Features.Auth.Commands.ForgetPassword
{
    public class ForgotPasswordCommandHandler(IAuthIdentityService authService) : IRequestHandler<ForgotPasswordCommand, Unit>
    {
        public async Task<Unit> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            await authService.ForgotPasswordAsync(request.Dto);
            return Unit.Value;
        }
    }
}
