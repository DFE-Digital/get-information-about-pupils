﻿namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;

/// <summary>
/// Represents a request for retrieving news articles.
/// </summary>
/// <param name="IsArchived">Indicates whether to fetch archived news articles.</param>
/// <param name="IsDraft">Indicates whether to fetch draft news articles.</param>
public record GetNewsArticlesRequest(bool IsArchived, bool IsDraft);
