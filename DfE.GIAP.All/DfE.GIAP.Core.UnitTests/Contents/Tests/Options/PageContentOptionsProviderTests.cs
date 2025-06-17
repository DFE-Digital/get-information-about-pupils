using DfE.GIAP.Core.Contents.Application.Options;
using DfE.GIAP.Core.Contents.Application.Options.Provider;
using DfE.GIAP.Core.UnitTests.TestDoubles;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.Contents.Tests.Options;
public sealed class PageContentOptionsProviderTests
{
    [Fact]
    public void PageOptionsContentProvider_Constructor_ThrowsNullException_When_CreatedWithNullOptions()
    {
        Action construct = () => new PageContentOptionProvider(null!);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void PageOptionsContentProvider_Constructor_ThrowsNullException_When_CreatedWithNullOptionsValue()
    {
        IOptions<PageContentOptions> nullValueOptions = OptionsTestDoubles.WithNullValue<PageContentOptions>();
        Action construct = () => new PageContentOptionProvider(nullValueOptions);
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void PageOptionsContentProvider_ThrowsException_When_PageKeyIsUnknown()
    {
        // Arrange
        Mock<IOptions<PageContentOptions>> mockOptions = new();
        string testPageKey = "dummy-page-key";
        PageContentOptions mockContentOptions = new();
        mockOptions.Setup(m => m.Value).Returns(mockContentOptions);

        PageContentOptionProvider sut = new(mockOptions.Object);

        // Act
        Action act = () => sut.GetPageContentOptionWithPageKey(testPageKey);

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}
