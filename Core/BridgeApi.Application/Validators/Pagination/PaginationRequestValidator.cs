using BridgeApi.Application.Abstractions.Pagination;
using FluentValidation;

namespace BridgeApi.Application.Validators.Pagination;

public class PaginationRequestValidator : AbstractValidator<PaginationRequest>
{
    public PaginationRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page must be at least 1.");

        RuleFor(x => x.Size)
            .InclusiveBetween(1, 50)
            .WithMessage("Size must be between 1 and 50.");
    }
}
