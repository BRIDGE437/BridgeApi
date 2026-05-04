using BridgeApi.Application.Features.Commands.TalentProfile.Upsert;
using FluentValidation;

namespace BridgeApi.Application.Validators.TalentProfile;

public class UpsertTalentProfileCommandValidator : AbstractValidator<UpsertTalentProfileCommandRequest>
{
    public UpsertTalentProfileCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Headline).NotEmpty().MaximumLength(120);
        RuleFor(x => x.YearsOfExperience).InclusiveBetween(0, 70);
        RuleFor(x => x.ExpectedSalaryMonthlyUsd).GreaterThanOrEqualTo(0).When(x => x.ExpectedSalaryMonthlyUsd.HasValue);

        RuleFor(x => x.CurrentRole).MaximumLength(150).When(x => !string.IsNullOrEmpty(x.CurrentRole));
        RuleFor(x => x.CurrentCompany).MaximumLength(150).When(x => !string.IsNullOrEmpty(x.CurrentCompany));

        RuleFor(x => x.Skills)
            .Must(s => s == null || s.Length <= 20)
            .WithMessage("At most 20 skills allowed.");

        RuleForEach(x => x.Skills)
            .MaximumLength(50)
            .When(x => x.Skills != null);

        RuleFor(x => x.LookingFor)
            .Must(s => s == null || s.Length <= 5)
            .WithMessage("At most 5 employment types allowed.");

        RuleFor(x => x.InterestedSectors)
            .Must(s => s == null || s.Length <= 10)
            .WithMessage("At most 10 interested sectors allowed.");
    }
}
