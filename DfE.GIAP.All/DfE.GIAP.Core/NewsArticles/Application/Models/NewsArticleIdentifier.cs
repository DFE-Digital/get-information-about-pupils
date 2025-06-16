// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DfE.GIAP.Core.NewsArticles.Application.Models;

public record NewsArticleIdentifier
{
    public string Value { get; }
    private NewsArticleIdentifier(string value) => Value = value;

    public static NewsArticleIdentifier New() => new(Guid.NewGuid().ToString());
    public static NewsArticleIdentifier From(string value) => new(value);
}
