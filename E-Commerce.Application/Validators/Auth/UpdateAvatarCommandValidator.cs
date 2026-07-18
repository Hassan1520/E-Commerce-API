//using ECommerce.Application.Common;
//using ECommerce.Application.Features.Auth.Commands;
//using FluentValidation;

//namespace ECommerce.Application.Validators.Auth;

//public class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
//{
//    private static readonly string[] AllowedContentTypes =
//    {
//        "image/jpeg", "image/png", "image/webp"
//    };

//    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

//    public UpdateAvatarCommandValidator()
//    {
//        RuleFor(x => x.FileName)
//            .NotEmpty()
//            .Must(name => name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
//                       || name.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
//                       || name.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
//                       || name.EndsWith(".webp", StringComparison.OrdinalIgnoreCase))
//            .WithMessage("Only .jpg, .jpeg, .png, and .webp files are allowed.");

//        RuleFor(x => x.ContentType)
//            .Must(ct => AllowedContentTypes.Contains(ct))
//            .WithMessage("Invalid content type. Allowed: JPEG, PNG, WEBP.");

//        RuleFor(x => x.FileStream)
//            .Must(stream => stream.Length <= MaxFileSizeBytes)
//            .WithMessage("Avatar size must not exceed 2 MB.");

//        RuleFor(x => x)
//            .MustAsync(async (cmd, cancellation) =>
//                await FileSignatureChecker.IsValidImageSignatureAsync(cmd.FileStream, cmd.ContentType))
//            .WithMessage("File content does not match the declared image type.");
//    }
//}
