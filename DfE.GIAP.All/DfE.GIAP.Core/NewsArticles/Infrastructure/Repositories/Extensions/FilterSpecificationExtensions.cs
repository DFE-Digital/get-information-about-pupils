// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles.Application.Models;

namespace DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.Extensions;
internal static class FilterSpecificationQueryExtensions
{
    internal static string ToNewsArticlesQuery(this IFilterSpecification<NewsArticle> filterSpecification)
    {
        string articleAlias = "c";
        string filterQuery = filterSpecification.ToFilterQuery(articleAlias);
        string filter = string.IsNullOrEmpty(filterQuery) ? string.Empty : $"AND {filterQuery}";

        string query = $"SELECT * FROM {articleAlias} WHERE {articleAlias}.DOCTYPE=7 {filter}";
        return query;
    }
}
