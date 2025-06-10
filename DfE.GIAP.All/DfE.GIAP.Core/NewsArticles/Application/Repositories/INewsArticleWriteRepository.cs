// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Application.Repositories;

/// <summary>
/// Defines methods for creating and managing news articles in a data store.
/// </summary>
/// <remarks>This interface is intended for write operations related to news articles. Implementations should
/// handle persistence and validation of the provided data.</remarks>
public interface INewsArticleWriteRepository
{
    /// <summary>
    /// Asynchronously creates a new news article in the system.
    /// </summary>
    /// <param name="newsArticle">The <see cref="NewsArticle"/> object containing the details of the article to be created. Cannot be null.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the created <see cref="NewsArticle"/>
    /// object,  or <see langword="null"/> if the creation was unsuccessful.</returns>
    Task<NewsArticle?> CreateNewsArticleAsync(NewsArticle newsArticle);
}
