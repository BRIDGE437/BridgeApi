using BridgeApi.Application.Features.Commands.File.UploadFile;
using BridgeApi.Domain.Enums;
using FluentValidation;

namespace BridgeApi.Application.Validators.File;

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommandRequest>
{
    private static readonly string[] ImageExtensions = [".jpg", ".jpeg", ".png", ".webp", ".gif"];
    private static readonly string[] DocumentExtensions = [".pdf", ".docx", ".pptx", ".xlsx"];
    private const long MaxImageSize = 5 * 1024 * 1024; // 5MB
    private const long MaxDocumentSize = 20 * 1024 * 1024; // 20MB

    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.");

        RuleFor(x => x.FileStream)
            .NotNull().WithMessage("File stream is required.");

        RuleFor(x => x.Size)
            .GreaterThan(0).WithMessage("File size must be greater than 0.");

        RuleFor(x => x.UploadedByUserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid file category.");

        RuleFor(x => x)
            .Must(x => HasValidExtension(x.FileName, x.Category))
            .WithMessage(x => x.Category == FileCategory.Document
                ? $"Invalid file extension. Allowed: {string.Join(", ", DocumentExtensions)}"
                : $"Invalid file extension. Allowed: {string.Join(", ", ImageExtensions)}");

        RuleFor(x => x)
            .Must(x => HasValidSize(x.Size, x.Category))
            .WithMessage(x => x.Category == FileCategory.Document
                ? $"File size exceeds the maximum limit of {MaxDocumentSize / (1024 * 1024)}MB for documents."
                : $"File size exceeds the maximum limit of {MaxImageSize / (1024 * 1024)}MB for images.");
    }

    private static bool HasValidExtension(string fileName, FileCategory category)
    {
        if (string.IsNullOrEmpty(fileName))
            return true; // NotEmpty rule handles this

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return category == FileCategory.Document
            ? DocumentExtensions.Contains(extension)
            : ImageExtensions.Contains(extension);
    }

    private static bool HasValidSize(long size, FileCategory category)
    {
        if (size <= 0)
            return true; // GreaterThan rule handles this

        return category == FileCategory.Document
            ? size <= MaxDocumentSize
            : size <= MaxImageSize;
    }
}
