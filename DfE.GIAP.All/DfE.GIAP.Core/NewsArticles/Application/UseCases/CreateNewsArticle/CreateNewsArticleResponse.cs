// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;

/// <summary>
/// Represents the response returned after creating a news article.
/// </summary>
/// <remarks>This response contains the created news article, if the operation was successful.</remarks>
/// <param name="NewsArticle">The news article that was created. This value will be <see langword="null"/> if the creation operation failed.</param>
public record CreateNewsArticleResponse(NewsArticle? NewsArticle);
