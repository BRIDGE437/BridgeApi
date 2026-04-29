using BridgeApi.Application.Features.Commands.InvestorProfile.Upsert;
using FluentValidation;

namespace BridgeApi.Application.Validators.InvestorProfile;

public class UpsertInvestorProfileCommandValidator : AbstractValidator<UpsertInvestorProfileCommandRequest>
{
    public UpsertInvestorProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.FirmName).MaximumLength(150).When(x => !string.IsNullOrEmpty(x.FirmName));
        RuleFor(x => x.InvestmentThesis).MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.InvestmentThesis));

        RuleFor(x => x.CheckSizeMinUsd).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CheckSizeMaxUsd).GreaterThanOrEqualTo(0);
        RuleFor(x => x)
            .Must(x => x.CheckSizeMaxUsd >= x.CheckSizeMinUsd)
            .WithMessage("Max check size must be >= min check size.")
            .WithName("CheckSize");

        RuleFor(x => x.PortfolioCompanyCount).GreaterThanOrEqualTo(0).When(x => x.PortfolioCompanyCount.HasValue);

        RuleFor(x => x.PreferredSectors)
            .Must(s => s == null || s.Length <= 10)
            .WithMessage("At most 10 preferred sectors allowed.");

        RuleFor(x => x.PreferredStages)
            .Must(s => s == null || s.Length <= 6)
            .WithMessage("At most 6 preferred stages allowed.");

        RuleFor(x => x.PreferredGeographies)
            .Must(g => g == null || g.Length <= 20)
            .WithMessage("At most 20 preferred geographies allowed.");

        RuleFor(x => x.FirmWebsite)
            .Must(x => string.IsNullOrEmpty(x) || Uri.TryCreate(x, UriKind.Absolute, out _))
            .WithMessage("Firm website must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.FirmWebsite));
    }
}
