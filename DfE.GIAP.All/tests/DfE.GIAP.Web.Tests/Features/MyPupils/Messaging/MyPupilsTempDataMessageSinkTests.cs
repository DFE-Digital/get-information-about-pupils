using DfE.GIAP.Core.Common.CrossCutting;
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

    [Theory]
    [MemberData(nameof(GetMessagesObjectInTempDataInputs))]
    public void GetMessages_Returns_Empty_When_ITempDataDictionary_Returns_Null_For_LookupKey(object? stored)
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[options.Value.MessagesKey]).Returns(stored);

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

    public static TheoryData<object> GetMessagesObjectInTempDataInputs
    {
        get
        {
            return new([
                null!,
                string.Empty,
                " ",
                "\n",
                new StoredObjectInTempData(),
                
            ]);
        }
    }

    public sealed class StoredObjectInTempData { }

    [Fact]
    public void GetMessages_Returns_Messages_And_Calls_Mapper_For_Each_Dto()
    {
        const string json = @"
                [
                    {
                        ""MessageLevel"": 0,
                        ""Message"": ""message a"",
                        ""Id"": ""1""
                    },
                    {
                        ""MessageLevel"": 1,
                        ""Message"": ""message b"",
                        ""Id"": ""2""
                    },
                    {
                        ""MessageLevel"": 2,
                        ""Message"": ""message c"",
                        ""Id"": ""3""
                    },
                ]
        ";
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[options.Value.MessagesKey]).Returns(json);

        Mock<ITempDataDictionaryProvider> providerMock = new();
        providerMock.Setup(t => t.GetTempData()).Returns(tempDataMock.Object);

        Mock<IMapper<MyPupilsMessageDto, MyPupilsMessage>> mapperFromDtoMock = MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>();
        mapperFromDtoMock
            .Setup(t => t.Map(It.IsAny<MyPupilsMessageDto>()))
            .Returns<MyPupilsMessageDto>(dto => new MyPupilsMessage(dto.Id, dto.MessageLevel, dto.Message));

        MyPupilsTempDataMessageSink sut = new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            mapperFromDtoMock.Object,
            options,
            providerMock.Object);

        // Act
        IReadOnlyList<MyPupilsMessage> response = sut.GetMessages();

        // Assert
        List<MyPupilsMessage> expectedMessages = [
            new("1", MessageLevel.Debug, "message a"),
            new("2", MessageLevel.Info, "message b"),
            new("3", MessageLevel.Error, "message c"),
        ];

        Assert.NotNull(response);
        Assert.Equivalent(expectedMessages, response);

        mapperFromDtoMock.Verify(
            mapper => mapper.Map(It.IsAny<MyPupilsMessageDto>()), Times.Exactly(3));

        tempDataMock.Verify(tempData => tempData[options.Value.MessagesKey], Times.Once);

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);
    }
}
