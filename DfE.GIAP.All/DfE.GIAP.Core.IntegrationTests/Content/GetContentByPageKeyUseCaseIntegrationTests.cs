using DfE.GIAP.Core.Content;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.Content.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DfE.GIAP.Core.IntegrationTests.Content;
[Collection(IntegrationTestCollectionMarker.Name)]
public sealed class GetContentByPageKeyUseCaseIntegrationTests : IAsyncLifetime
{
    private readonly CosmosDbFixture _dbFixture;

    public GetContentByPageKeyUseCaseIntegrationTests(CosmosDbFixture dbFixture)
    {
        _dbFixture = dbFixture;
    }

    public async Task InitializeAsync() => await _dbFixture.Database.ClearDatabaseAsync();
    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetContentByPageKeyUseCase_RetrievesPageContentKey_When_PageKeyIsFound()
    {
        // Arrange
        List<ContentDTO> content = ContentDTOTestDoubles.Generate(count: 10);
        content[0].id = "DocumentId1";
        await Parallel.ForEachAsync(content, async (t, ctx) => await _dbFixture.Database.WriteAsync(t));

        Dictionary<string, string> contentRepositoryOptions = new()
        {
            ["PageContentOptions:TestPage1:DocumentId"] = "DocumentId1",
        };

        IConfiguration configuration = ConfigurationTestDoubles.Default()
            .WithConfiguration(contentRepositoryOptions)
            .WithLocalCosmosDb()
            .Build();

        IServiceCollection services = ServiceCollectionTestDoubles.Default()
            .AddSharedDependencies()
            .RemoveAll<IConfiguration>() // replace default configuration
            .AddSingleton<IConfiguration>(configuration)
            .AddContentDependencies();

        IServiceProvider provider = services.BuildServiceProvider();
        using IServiceScope scope = provider.CreateScope();

        IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse> sut =
            scope.ServiceProvider.GetService<IUseCase<GetContentByPageKeyUseCaseRequest, GetContentByPageKeyUseCaseResponse>>()!;

        // Act
        GetContentByPageKeyUseCaseRequest request = new(pageKey: "TestPage1");
        GetContentByPageKeyUseCaseResponse response = await sut.HandleRequest(request);

        // Assert
        Assert.NotNull(response);
        Assert.NotNull(response.Content);
        Assert.Equal(content[0].Title, response.Content!.Title);
        Assert.Equal(content[0].Body, response.Content!.Body);
    }
}
