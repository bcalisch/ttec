using FluentValidation;

namespace Ticketing.Api.Contracts.KnowledgeArticles;

public class UpdateKnowledgeArticleRequestValidator : AbstractValidator<UpdateKnowledgeArticleRequest>
{
    public UpdateKnowledgeArticleRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).MaximumLength(10000);
        RuleFor(x => x.Tags).MaximumLength(500);
    }
}
