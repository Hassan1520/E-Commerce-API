using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.ResetPassword
{
    public record ResetPasswordCommand(ResetPasswordRequestDto Dto) : IRequest<Unit>;

}
