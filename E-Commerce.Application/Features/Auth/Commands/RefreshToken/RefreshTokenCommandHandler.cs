using ECommerce.Application.DTOs.Auth;
using ECommerce.Application.Interfaces.Services;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler(IAuthIdentityService authService) : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
    {
        public Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
            => authService.RefreshTokenAsync(request.Dto);
    }
}
