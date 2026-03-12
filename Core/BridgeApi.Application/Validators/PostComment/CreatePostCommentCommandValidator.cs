using BridgeApi.Application.Features.Commands.PostComment.CreatePostComment;
using FluentValidation;

namespace BridgeApi.Application.Validators.PostComment;

public class CreatePostCommentCommandValidator : AbstractValidator<CreatePostCommentCommandRequest>
{
    public CreatePostCommentCommandValidator()
    {
        RuleFor(x => x.PostId)
            .NotEmpty().WithMessage("Post ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.CommentText)
            .NotEmpty().WithMessage("Comment text is required.")
            .MaximumLength(1000).WithMessage("Comment text must not exceed 1000 characters.");
    }
}
