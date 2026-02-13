using DfE.GIAP.Web.Shared.TempData;
using DfE.GIAP.Web.Tests.Shared.HttpContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DfE.GIAP.Web.Tests.Shared.TempData;

public sealed class TempDataDictionaryProviderTests
{
    [Fact]
    public void Constructor_Throws_When_HttpContextAccessor_Is_Null()
    {
        // Arrange
        Func<TempDataDictionaryProvider> construct = () =>
            new TempDataDictionaryProvider(
                httpContextAccessor: null!,
                tempDataDictionaryFactory: new Mock<ITempDataDictionaryFactory>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_TempDataDictionaryFactory_Is_Null()
    {
        // Arrange
        Func<TempDataDictionaryProvider> construct = () =>
            new TempDataDictionaryProvider(
                httpContextAccessor: new Mock<IHttpContextAccessor>().Object,
                tempDataDictionaryFactory: null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void GetTempData_Throws_When_HttpContext_Is_Null()
    {
        // Arrange
        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        httpContextAccessorMock.Setup(x => x.HttpContext).Returns<Microsoft.AspNetCore.Http.HttpContext>(null!);

        Mock<ITempDataDictionaryFactory> tempDataDictionaryFactoryMock = new();
        TempDataDictionaryProvider sut = new(
            httpContextAccessorMock.Object,
            tempDataDictionaryFactoryMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(() => sut.GetTempData());

        tempDataDictionaryFactoryMock.Verify(
            (factory) => factory.GetTempData(It.IsAny<Microsoft.AspNetCore.Http.HttpContext>()),
                Times.Never);
    }

    [Fact]
    public void GetTempData_Returns_TempDataDictionary_When_HttpContext_Is_Present()
    {
        // Arrange
        Microsoft.AspNetCore.Http.HttpContext stubHttpContext = HttpContextTestDoubles.Stub();

        Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        httpContextAccessorMock
            .Setup(x => x.HttpContext)
            .Returns(stubHttpContext);

        Mock<ITempDataDictionaryFactory> tempDataDictionaryFactoryMock = new();
        Mock<ITempDataDictionary> tempDataDictionaryMock = new();

        tempDataDictionaryFactoryMock
            .Setup(x => x.GetTempData(stubHttpContext))
            .Returns(tempDataDictionaryMock.Object);

        TempDataDictionaryProvider sut = new(httpContextAccessorMock.Object, tempDataDictionaryFactoryMock.Object);

        // Act Assert
        ITempDataDictionary result = sut.GetTempData();

        Assert.NotNull(result);
        Assert.Same(tempDataDictionaryMock.Object, result);

        tempDataDictionaryFactoryMock.Verify(
            (factory) => factory.GetTempData(stubHttpContext),
                Times.Once);
    }
}
