using FluentValidation;

namespace Ticketing.Api.Contracts.KnowledgeArticles;

public class CreateKnowledgeArticleRequestValidator : AbstractValidator<CreateKnowledgeArticleRequest>
{
    public CreateKnowledgeArticleRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).MaximumLength(10000);
        RuleFor(x => x.Tags).MaximumLength(500);
    }
}
