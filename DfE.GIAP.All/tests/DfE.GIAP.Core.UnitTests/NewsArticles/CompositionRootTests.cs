using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.NewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.Runtime;
using DfE.GIAP.SharedTests.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using CompositionRoot = DfE.GIAP.Core.NewsArticles.CompositionRoot;

namespace DfE.GIAP.Core.UnitTests.NewsArticles;

public sealed class CompositionRootTests
{
    [Fact]
    public void ThrowsArgumentNullException_When_ServicesIsNull()
    {
        IServiceCollection? serviceCollection = null;
        Action register = () => CompositionRoot.AddNewsArticleDependencies(serviceCollection!);
        Assert.Throws<ArgumentNullException>(register);
    }

    [Fact]
    public void Registers_CompositionRoot_CanResolve_Services()
    {
        // Arrange
        IServiceCollection services =
            ServiceCollectionTestDoubles.Default()
                .AddAspNetCoreRuntimeProvidedServices()
                .AddFeaturesSharedServices()
                .AddNewsArticleDependencies();

        // Act
        IServiceProvider provider = services.BuildServiceProvider();

        // Assert
        Assert.NotNull(provider);

        Assert.NotNull(provider.GetService<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>>());
        Assert.NotNull(provider.GetService<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<CreateNewsArticleRequest>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<DeleteNewsArticleRequest>>());
        Assert.NotNull(provider.GetService<IUseCaseRequestOnly<UpdateNewsArticleRequest>>());

        Assert.NotNull(provider.GetService<IMapper<NewsArticleDto, NewsArticle>>());
        Assert.NotNull(provider.GetService<INewsArticleReadOnlyRepository>());
        Assert.NotNull(provider.GetService<INewsArticleWriteOnlyRepository>());
    }
}
