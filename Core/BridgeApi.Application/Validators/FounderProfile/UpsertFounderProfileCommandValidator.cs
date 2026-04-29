using BridgeApi.Application.Features.Commands.FounderProfile.Upsert;
using FluentValidation;

namespace BridgeApi.Application.Validators.FounderProfile;

public class UpsertFounderProfileCommandValidator : AbstractValidator<UpsertFounderProfileCommandRequest>
{
    public UpsertFounderProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.StartupName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OneLiner).MaximumLength(160).When(x => !string.IsNullOrEmpty(x.OneLiner));
        RuleFor(x => x.ProblemStatement).MaximumLength(500).When(x => !string.IsNullOrEmpty(x.ProblemStatement));
        RuleFor(x => x.Country).MaximumLength(100).When(x => !string.IsNullOrEmpty(x.Country));

        RuleFor(x => x.FundingNeedUsd).GreaterThanOrEqualTo(0).When(x => x.FundingNeedUsd.HasValue);
        RuleFor(x => x.TeamSize).GreaterThanOrEqualTo(0).When(x => x.TeamSize.HasValue);
        RuleFor(x => x.FoundedYear)
            .InclusiveBetween(1900, DateTime.UtcNow.Year)
            .When(x => x.FoundedYear.HasValue);

        RuleFor(x => x.SecondarySectors)
            .Must(s => s == null || s.Length <= 5)
            .WithMessage("At most 5 secondary sectors allowed.");

        RuleFor(x => x.StartupWebsite)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Startup website must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.StartupWebsite));

        RuleFor(x => x.PitchDeckUrl)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Pitch deck URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.PitchDeckUrl));
    }
}
