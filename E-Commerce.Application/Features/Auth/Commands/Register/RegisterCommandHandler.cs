using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler(IAuthIdentityService authService) : IRequestHandler<RegisterCommand, AuthResponseDto>
    {
        public Task<AuthResponseDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            return authService.RegisterAsync(request.Dto);
        }
    }
}
