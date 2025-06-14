// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace DfE.GIAP.Core.UnitTests.NewsArticles.UseCases;
internal static class NewsArticleWriteOnlyRepositoryTestDoubles
{
    internal static Mock<INewsArticleWriteRepository> Default() => CreateMock();

    private static Mock<INewsArticleWriteRepository> CreateMock() => new();
}
