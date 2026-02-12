using FluentValidation;

namespace Ticketing.Api.Contracts.Comments;

public class CreateCommentRequestValidator : AbstractValidator<CreateCommentRequest>
{
    public CreateCommentRequestValidator()
    {
        RuleFor(x => x.Body).NotEmpty().MaximumLength(4000);
    }
}
