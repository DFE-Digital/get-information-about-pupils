using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;

public class UpdateNewsArticleUseCase : IUseCaseRequestOnly<UpdateNewsArticleRequest>
{
    private readonly INewsArticleWriteOnlyRepository _newsArticleWriteRepository;
    private readonly IMapper<UpdateNewsArticlesRequestProperties, NewsArticle> _mapper;

    public UpdateNewsArticleUseCase(INewsArticleWriteOnlyRepository newsArticleWriteRepository, IMapper<UpdateNewsArticlesRequestProperties, NewsArticle> mapper)
    {
        ArgumentNullException.ThrowIfNull(newsArticleWriteRepository);
        ArgumentNullException.ThrowIfNull(mapper);
        _newsArticleWriteRepository = newsArticleWriteRepository;
        _mapper = mapper;
    }

    public async Task HandleRequestAsync(UpdateNewsArticleRequest request)
    {
        // Validate request
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.UpdateArticleProperties);

        NewsArticle articleToUpdate = _mapper.Map(request.UpdateArticleProperties);

        await _newsArticleWriteRepository.UpdateNewsArticleAsync(articleToUpdate);
    }
}

internal sealed class UpdateNewsArticlesRequestPropertiesMapperToNewsArticle : IMapper<UpdateNewsArticlesRequestProperties, NewsArticle>
{
    public NewsArticle Map(UpdateNewsArticlesRequestProperties input)
    {
        ArgumentNullException.ThrowIfNull(input);
        return new()
        {
            Id = input.Id,
            Title = input.Title?.Value ?? string.Empty,
            Body = input.Body?.Value ?? string.Empty,
            Pinned = input.Pinned ?? false,
            Published = input.Published ?? false,
            CreatedDate = input.CreatedDate,
            ModifiedDate = input.ModifiedDate,
        };
    }
}
