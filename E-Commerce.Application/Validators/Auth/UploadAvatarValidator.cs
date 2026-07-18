using ECommerce.Application.Common;
using ECommerce.Application.Features.Auth.Commands.UpdateAvatar;
using FluentValidation;

namespace ECommerce.Application.Validators.Auth;

public class UploadAvatarValidator : AbstractValidator<UpdateAvatarCommand>
{
    private static readonly string[] AllowedContentTypes =
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    private static readonly string[] AllowedExtensions =
    {
        ".jpg", ".jpeg", ".png", ".webp"
    };

    private const long MaxFileSizeBytes = 2 * 1024 * 1024;

    public UploadAvatarValidator()
    {
        RuleFor(x => x.FileStream)
        .NotNull()
        .WithMessage("No file uploaded.");

        When(x => x.FileStream is not null, () =>
        {
            RuleFor(x => x.FileStream.Length)
                .GreaterThan(0)
                    .WithMessage("Uploaded file is empty.")
                .LessThanOrEqualTo(MaxFileSizeBytes)
                    .WithMessage($"File size exceeds the {MaxFileSizeBytes / (1024 * 1024)}MB limit.");

            RuleFor(x => x.ContentType)
                .Must(ct => AllowedContentTypes.Contains(ct))
                .WithMessage("Only JPEG, PNG, and WEBP images are allowed.");

            RuleFor(x => x.FileName)
                .Must(fn => AllowedExtensions.Contains(
                    Path.GetExtension(fn).ToLowerInvariant()))
                .WithMessage("Invalid file extension.");

            RuleFor(x => x)
                .MustAsync(async (file, _) =>
                    await FileSignatureChecker.IsValidImageSignatureAsync(
                        file.FileStream, file.ContentType))
                .WithMessage(
                    "File content does not match the declared type. " +
                    "The file may be corrupted or disguised.")
                .When(x => AllowedContentTypes.Contains(x.ContentType)
                         && x.FileStream.Length > 0);
        });

    }
}