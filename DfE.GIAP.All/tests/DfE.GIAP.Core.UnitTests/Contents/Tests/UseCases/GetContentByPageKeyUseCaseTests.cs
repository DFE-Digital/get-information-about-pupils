using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Contents.Application.Options;
using DfE.GIAP.Core.Contents.Application.Options.Provider;
using DfE.GIAP.Core.Contents.Application.Repositories;
using DfE.GIAP.Core.Contents.Application.UseCases.GetContentByPageKeyUseCase;
using DfE.GIAP.Core.UnitTests.Contents.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Contents.Tests.UseCases;
public sealed class GetContentByPageKeyUseCaseTests
{
    private static readonly string StubValidPageKey = "valid-pagekey";

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
    public async Task GetContentByPageKeyUseCase_HandleRequest_BubblesException_When_ProviderThrows()
    {
        IContentReadOnlyRepository mockRepository = ContentReadOnlyRepositoryTestDoubles.Default().Object;
        Mock<IPageContentOptionsProvider> mockProvider = new();

        mockProvider
            .Setup((t) => t.GetPageContentOptionWithPageKey(It.IsAny<string>()))
            .Returns(() => throw new Exception("test exception"));

        GetContentByPageKeyUseCase sut = new(mockProvider.Object, mockRepository);
        GetContentByPageKeyUseCaseRequest request = new(StubValidPageKey);
        Func<Task<GetContentByPageKeyUseCaseResponse>> act = () => sut.HandleRequestAsync(request);
        await Assert.ThrowsAsync<Exception>(act);
    }

    [Fact]
    public async Task GetContentByPageKeyUseCase_HandleRequest_BubblesException_When_RepositoryThrows()
    {
        Mock<IContentReadOnlyRepository> mockRepository = ContentReadOnlyRepositoryTestDoubles.Default();
        Mock<IPageContentOptionsProvider> mockProvider = new();

        PageContentOption option = PageContentOptionTestDoubles.StubFor("documentId");

        mockProvider
            .Setup((t) => t.GetPageContentOptionWithPageKey(It.IsAny<string>()))
            .Returns(option);

        mockRepository
            .Setup((t) => t.GetContentByIdAsync(It.IsAny<ContentKey>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => throw new Exception("Repository exception"));

        GetContentByPageKeyUseCase sut = new(mockProvider.Object, mockRepository.Object);
        GetContentByPageKeyUseCaseRequest request = new(StubValidPageKey);
        Func<Task<GetContentByPageKeyUseCaseResponse>> act = () => sut.HandleRequestAsync(request);
        await Assert.ThrowsAsync<Exception>(act);
    }

    [Fact]
    public async Task GetContentByPageKeyUseCase_HandleRequest_ReturnsContent_When_PageKeyIsValid()
    {
        // Arrange
        string validPageKey = "test-pagekey";
        Mock<IContentReadOnlyRepository> mockRepository = ContentReadOnlyRepositoryTestDoubles.Default();
        Mock<IPageContentOptionsProvider> mockContentOptionProvider = new();
        PageContentOption pageContentOption = new()
        {
            DocumentId = "test-documentid-1"
        };

        Content content = new()
        {
            Title = "Test title",
            Body = "Test content"
        };

        mockContentOptionProvider
            .Setup(t => t.GetPageContentOptionWithPageKey(It.Is<string>(t => t == validPageKey)))
            .Returns(pageContentOption)
            .Verifiable();

        mockRepository.Setup(t => t.GetContentByIdAsync(
            It.IsAny<ContentKey>(),
            It.IsAny<CancellationToken>()))
                .ReturnsAsync(content);

        GetContentByPageKeyUseCase sut = new(mockContentOptionProvider.Object, mockRepository.Object);
        GetContentByPageKeyUseCaseRequest request = new(pageKey: validPageKey);
        GetContentByPageKeyUseCaseResponse response = await sut.HandleRequestAsync(request);


        Assert.NotNull(response);
        Assert.Equal<Content>(content, response.Content);

        mockContentOptionProvider.Verify((provider) =>
            provider.GetPageContentOptionWithPageKey(It.Is<string>((t) => t == validPageKey)),
                Times.Once());

        mockRepository.Verify((repository) =>
            repository.GetContentByIdAsync(It.IsAny<ContentKey>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
