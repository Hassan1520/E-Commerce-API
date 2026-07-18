using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler(IAuthIdentityService authService) : IRequestHandler<LoginCommand, AuthResponseDto>
    {
        public Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
            => authService.LoginAsync(request.Dto);
    }
}
