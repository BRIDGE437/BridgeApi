using BridgeApi.Application.Features.Commands.Post.CreatePost;
using FluentValidation;

namespace BridgeApi.Application.Validators.Post;

public class CreatePostCommandValidator : AbstractValidator<CreatePostCommandRequest>
{
    public CreatePostCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Content)
            .MaximumLength(5000).WithMessage("Content must not exceed 5000 characters.")
            .When(x => !string.IsNullOrEmpty(x.Content));

        RuleFor(x => x.ImageUrl)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Image URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.ImageUrl));
    }
}
