// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
public record CreateNewsArticleRequest(string Title, string Body, bool Published, bool Archived, bool Pinned);
