using MediatR;

namespace ECommerce.Application.Features.Auth.Commands.UpdateAvatar
{
    public record UpdateAvatarCommand(int UserId, Stream FileStream, string FileName, string ContentType) : IRequest<string>;

}
