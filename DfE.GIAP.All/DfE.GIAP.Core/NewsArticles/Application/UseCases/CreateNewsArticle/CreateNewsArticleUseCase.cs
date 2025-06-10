// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.Repositories;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
public class CreateNewsArticleUseCase : IUseCase<CreateNewsArticleRequest, CreateNewsArticleResponse>
{
    private readonly INewsArticleWriteRepository _newsArticleWriteRepository;
    public CreateNewsArticleUseCase(INewsArticleWriteRepository newsArticleWriteRepository)
    {
        _newsArticleWriteRepository = newsArticleWriteRepository ??
            throw new ArgumentNullException(nameof(newsArticleWriteRepository));
    }

    public async Task<CreateNewsArticleResponse> HandleRequest(CreateNewsArticleRequest request)
    {
        // Validate the request

        NewsArticle newsArticle = NewsArticle.Create(
            title: request.Title,
            body: request.Body,
            published: request.Published,
            archived: request.Archived,
            pinned: request.Pinned);

        NewsArticle? createResponse = await _newsArticleWriteRepository.CreateNewsArticleAsync(newsArticle);

        CreateNewsArticleResponse response = new(createResponse);
        return response;
    }
}
