using BridgeApi.Application.Features.Commands.UserProfile.CreateUserProfile;
using FluentValidation;

namespace BridgeApi.Application.Validators.UserProfile;

public class CreateUserProfileCommandValidator : AbstractValidator<CreateUserProfileCommandRequest>
{
    public CreateUserProfileCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Name));

        RuleFor(x => x.Surname)
            .MaximumLength(100).WithMessage("Surname must not exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Surname));

        RuleFor(x => x.Bio)
            .MaximumLength(500).WithMessage("Bio must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Bio));

        RuleFor(x => x.LinkedInUrl)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("LinkedIn URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.LinkedInUrl));

        RuleFor(x => x.GitHubUrl)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("GitHub URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.GitHubUrl));

        RuleFor(x => x.WebsiteUrl)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Website URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl));
    }
}
