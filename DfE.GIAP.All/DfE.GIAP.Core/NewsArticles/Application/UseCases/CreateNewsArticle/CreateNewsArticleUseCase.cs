// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
public class CreateNewsArticleUseCase : IUseCaseRequestOnly<CreateNewsArticleRequest>
{
    private readonly INewsArticleWriteRepository _newsArticleWriteRepository;
    public CreateNewsArticleUseCase(INewsArticleWriteRepository newsArticleWriteRepository)
    {
        _newsArticleWriteRepository = newsArticleWriteRepository ??
            throw new ArgumentNullException(nameof(newsArticleWriteRepository));
    }

    public async Task HandleRequestAsync(CreateNewsArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title cannot be null or empty.", nameof(request.Title));
        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body cannot be null or empty.", nameof(request.Body));

        NewsArticle newsArticle = NewsArticle.Create(
            title: request.Title,
            body: request.Body,
            published: request.Published,
            archived: request.Archived,
            pinned: request.Pinned);

        await _newsArticleWriteRepository.CreateNewsArticleAsync(newsArticle);
    }
}
