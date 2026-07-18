using ECommerce.Application.DTOs.Auth;
using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.ForgetPassword
{
    public record ForgotPasswordCommand(ForgotPasswordRequestDto Dto) : IRequest<Unit>;

}
