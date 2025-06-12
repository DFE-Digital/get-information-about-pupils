using DfE.GIAP.Core.Content.Application.Model;
using DfE.GIAP.Core.Content.Application.Options;
using DfE.GIAP.Core.Content.Application.Options.Provider;
using DfE.GIAP.Core.Content.Application.Repository;
using DfE.GIAP.Core.Content.Application.UseCases.GetContentByPageKeyUseCase;

namespace DfE.GIAP.Core.UnitTests.Content.UseCases;
public sealed class GetContentByPageKeyUseCaseTests
{

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    public void GetContentByPageKeyUseCaseRequest_ThrowsNullException_When_CreatedWithNullOrEmpty(string? pageKey)
    {
        Action construct = () => new GetContentByPageKeyUseCaseRequest(pageKey!);
        Assert.ThrowsAny<ArgumentException>(construct);
    }

    [Fact]
    public void GetContentByPageKeyUseCase_Constructor_ThrowsNullException_When_CreatedWithNullRepository()
    {
        Mock<IPageContentOptionsProvider> mockProvider = new();
        Action construct = () => new GetContentByPageKeyUseCase(mockProvider.Object, null!);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void GetContentByPageKeyUseCase_Constructor_ThrowsNullException_When_CreatedWithNull_PageContentOptionProvider()
    {
        IContentReadOnlyRepository mockRepository = ContentReadOnlyRepositoryTestDoubles.Default().Object;
        Action construct = () => new GetContentByPageKeyUseCase(null!, mockRepository);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task GetContentByPageKeyUseCase_BubblesException_When_ProviderThrows()
    {
        IContentReadOnlyRepository mockRepository = ContentReadOnlyRepositoryTestDoubles.Default().Object;
        Mock<IPageContentOptionsProvider> mockProvider = new();

        mockProvider.Setup(
            (t) => t.GetPageContentOptionWithPageKey(It.IsAny<string>()))
                .Returns(() => throw new Exception("test exception"));

        GetContentByPageKeyUseCase sut = new(mockProvider.Object, mockRepository);
        GetContentByPageKeyUseCaseRequest request = new("valid-pagekey");
        Func<Task<GetContentByPageKeyUseCaseResponse>> act = () => sut.HandleRequest(request);
        await Assert.ThrowsAsync<Exception>(act);
    }

    [Fact]
    public async Task GetContentByPageKeyUseCase_BubblesException_When_RepositoryThrows()
    {
        Mock<IContentReadOnlyRepository> mockRepository = ContentReadOnlyRepositoryTestDoubles.Default();
        Mock<IPageContentOptionsProvider> mockProvider = new();

        mockProvider.Setup(
            (t) => t.GetPageContentOptionWithPageKey(It.IsAny<string>()))
                .Returns(It.IsAny<PageContentOption>());

        mockRepository.Setup(
            t => t.GetContentByKeyAsync(It.IsAny<ContentKey>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => throw new Exception("Repository exception"));

        GetContentByPageKeyUseCase sut = new(mockProvider.Object, mockRepository.Object);
        GetContentByPageKeyUseCaseRequest request = new("valid-pagekey");
        Func<Task<GetContentByPageKeyUseCaseResponse>> act = () => sut.HandleRequest(request);
        await Assert.ThrowsAsync<Exception>(act);
    }

    [Fact]
    public async Task GetContentByPageKeyUseCase_HandleRequest_Calls_Provider_And_Repository_Once_ReturnsContent()
    {
        // Arrange
        string validPageKey = "test-pagekey";
        Mock<IContentReadOnlyRepository> mockRepository = ContentReadOnlyRepositoryTestDoubles.Default();
        Mock<IPageContentOptionsProvider> mockContentOptionProvider = new();
        PageContentOption option = new()
        {
            DocumentId = "test-documentid-1"
        };

        Core.Content.Application.Model.Content content = new()
        {
            Title = "Test title",
            Body = "Test content"
        };

        mockContentOptionProvider
            .Setup(t => t.GetPageContentOptionWithPageKey(It.Is<string>(t => t == validPageKey)))
            .Returns(option)
            .Verifiable();

        mockRepository.Setup(t => t.GetContentByKeyAsync(
            It.IsAny<ContentKey>(),
            It.IsAny<CancellationToken>()))
                .ReturnsAsync(content);

        GetContentByPageKeyUseCase sut = new(mockContentOptionProvider.Object, mockRepository.Object);
        GetContentByPageKeyUseCaseRequest request = new(pageKey: validPageKey);
        GetContentByPageKeyUseCaseResponse response = await sut.HandleRequest(request);


        Assert.NotNull(response);
        Assert.Equal(content, response.Content);

        mockContentOptionProvider.Verify(
            (provider) => provider.GetPageContentOptionWithPageKey(
                It.Is<string>((t) => t == validPageKey)),
                Times.Once());

        mockRepository.Verify(
            (repository) => repository.GetContentByKeyAsync(
                It.IsAny<ContentKey>(), It.IsAny<CancellationToken>()),
                Times.Once());
    }


}
