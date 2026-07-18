using ECommerce.Application.DTOs.Auth;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Features.Auth.Commands.Login
{
    public record LoginCommand(LoginRequestDto Dto) : IRequest<AuthResponseDto>;

}
