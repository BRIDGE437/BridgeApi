using BridgeApi.Application.Features.Commands.UserProfile.UpdateUserProfile;
using FluentValidation;

namespace BridgeApi.Application.Validators.UserProfile;

public class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommandRequest>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Name).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Name));
        RuleFor(x => x.Surname).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Surname));
        RuleFor(x => x.Title).MaximumLength(150).When(x => !string.IsNullOrEmpty(x.Title));
        RuleFor(x => x.Headline).MaximumLength(200).When(x => !string.IsNullOrEmpty(x.Headline));
        RuleFor(x => x.Bio).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.Bio));
        RuleFor(x => x.Location).MaximumLength(150).When(x => !string.IsNullOrEmpty(x.Location));
        RuleFor(x => x.PhoneNumber).MaximumLength(40).When(x => !string.IsNullOrEmpty(x.PhoneNumber));

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
