using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Shared.TempData;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Messaging;
public sealed class MyPupilsTempDataMessageSinkTests
{
    [Fact]
    public void Constructor_Throws_When_ToDataTransferObjectMapper_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            null!,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            new Mock<ITempDataDictionaryProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_FromDataTransferObjectMapper_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            null!,
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            new Mock<ITempDataDictionaryProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Options_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            null!,
            new Mock<ITempDataDictionaryProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_OptionsValue_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            OptionsTestDoubles.MockNullOptions<MyPupilsMessagingOptions>(),
            new Mock<ITempDataDictionaryProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_TempDataProvider_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void GetMessages_Returns_Empty_When_ITempDataDictionary_Returns_Null_For_LookupKey()
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[options.Value.MessagesKey]).Returns(null!);

        Mock<ITempDataDictionaryProvider> providerMock = new();
        providerMock.Setup(t => t.GetTempData()).Returns(tempDataMock.Object);

        MyPupilsTempDataMessageSink sut = new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            options,
            providerMock.Object);

        // Act
        IReadOnlyList<MyPupilsMessage> response = sut.GetMessages();

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);

        tempDataMock.Verify(tempData => tempData[options.Value.MessagesKey], Times.Once);

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);
    }
}
