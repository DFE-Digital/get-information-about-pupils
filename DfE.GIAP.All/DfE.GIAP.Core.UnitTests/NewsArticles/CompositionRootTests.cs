﻿using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DfE.GIAP.Core.UnitTests.NewsArticles;

public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        Action register = () => CompositionRoot.AddNewsArticleDependencies(serviceCollection);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange

        IServiceCollection services = ServiceCollectionTestDoubles.Default();
        services.AddSingleton(typeof(ILogger<>), typeof(InMemoryLogger<>));
        services.AddSingleton<IConfiguration>(sp => ConfigurationTestDoubles.WithRepositoryOptions());

        // Act
        IServiceCollection registeredServices = CompositionRoot.AddNewsArticleDependencies(services);
        IServiceProvider provider = registeredServices.BuildServiceProvider();

        // Assert
        Assert.NotNull(registeredServices);
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>());
        Assert.NotNull(provider.GetService<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>());
        Assert.NotNull(provider.GetService<IMapper<NewsArticleDTO, NewsArticle>>());
        Assert.NotNull(provider.GetService<INewsArticleReadRepository>());
        Assert.NotNull(provider.GetService<INewsArticleReadRepository>());
    }
}
