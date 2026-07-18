using ECommerce.Application.Interfaces.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerce.Application.Features.Auth.Commands.UpdateAvatar
{
    public class UpdateAvatarCommandHandler(IAuthIdentityService authService) : IRequestHandler<UpdateAvatarCommand, string>
    {
        public Task<string> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
            => authService.UpdateAvatarAsync(request.UserId, request.FileStream, request.FileName, request.ContentType);
    }
}
