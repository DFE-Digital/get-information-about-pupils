using DfE.GIAP.Core.Contents;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Contents.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.IntegrationTests.Contents;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetContentByPageKeyUseCaseIntegrationTests : IAsyncLifetime
{
    private readonly CosmosDbFixture _dbFixture;

    public GetContentByPageKeyUseCaseIntegrationTests(CosmosDbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    public async Task InitializeAsync() => await _dbFixture.Database!.ClearDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetContentByPageKeyUseCase_RetrievesPageContentKey_When_PageKeyIsFound()
    {
        // Arrange
        List<ContentDto> contentDtos = ContentDtoTestDoubles.Generate(count: 10);
        ContentDto targetContent = contentDtos[0];

        await _dbFixture.Database.WriteManyAsync(contentDtos);

        Dictionary<string, string> contentRepositoryOptions = new()
        {
            ["PageContentOptions:TestPage1:DocumentId"] = targetContent.id,
        };

        IConfiguration configuration = ConfigurationTestDoubles.Default()
            .WithConfiguration(contentRepositoryOptions)
            .WithLocalCosmosDb()
            .Build();

        IServiceCollection services = ServiceCollectionTestDoubles.Default()
            .AddSharedDependencies()
            .RemoveAll<IConfiguration>() // replace default configuration
            .AddSingleton(configuration)
            .AddContentDependencies();

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetContentByPageKeyRequest, GetContentByPageKeyResponse>>()!;

        // Act
        GetContentByPageKeyRequest request = new(pageKey: "TestPage1");
        GetContentByPageKeyResponse response = await sut.HandleRequestAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.Equal(contentDtos[0].Title, response.Content!.Title);
        Assert.Equal(contentDtos[0].Body, response.Content!.Body);
    }
}
