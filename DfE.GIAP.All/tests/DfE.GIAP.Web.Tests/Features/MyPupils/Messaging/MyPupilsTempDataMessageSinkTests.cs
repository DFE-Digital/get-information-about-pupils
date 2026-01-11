using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Shared.TempData;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Messaging;
public sealed class MyPupilsTempDataMessageSinkTests
{

    private static readonly string MESSAGES_SERIALISED_STUB = @"
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
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        Mock<ITempDataDictionary> tempDataMock = new();
        tempDataMock.Setup(t => t[options.Value.MessagesKey]).Returns(MESSAGES_SERIALISED_STUB);

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

    [Theory]
    [MemberData(nameof(AddMessageObjectsInTempDataInputs))]
    public void AddMessage_Appends_To_NullOrEmpty_Messages(object? stored)
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        Mock<ITempDataDictionary> tempDataMock = new();
        string? capturedJson = null;

        tempDataMock.Setup(
            (tempData) =>
                tempData.Peek(options.Value.MessagesKey)).Returns(stored);

        tempDataMock
            .SetupSet(tempData => tempData[options.Value.MessagesKey] = It.IsAny<object>())
            .Callback<string, object>((key, value) =>
            {
                capturedJson = value as string;
            });

        Mock<ITempDataDictionaryProvider> providerMock = new();
        providerMock.Setup(t => t.GetTempData()).Returns(tempDataMock.Object);

        Mock<IMapper<MyPupilsMessage, MyPupilsMessageDto>> mapperMock =
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>();

        mapperMock
            .Setup(t => t.Map(It.IsAny<MyPupilsMessage>()))
            .Returns<MyPupilsMessage>(
                (message) => new MyPupilsMessageDto()
                {
                    Id = message.Id,
                    Message = message.Message,
                    MessageLevel = message.Level
                });

        MyPupilsTempDataMessageSink sut = new(
            mapperMock.Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            options,
            providerMock.Object);

        // Act
        MyPupilsMessage messageStub = new(MessageLevel.Debug, "test");

        // Act
        sut.AddMessage(messageStub);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(capturedJson));

        List<MyPupilsMessageDto>? messages =
            JsonConvert.DeserializeObject<List<MyPupilsMessageDto>>(capturedJson!);

        Assert.NotNull(messages);
        Assert.Single(messages!);
        Assert.Equal(MessageLevel.Debug, messages![0].MessageLevel);
        Assert.Equal("test", messages![0].Message);

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);
        mapperMock.Verify(t => t.Map(It.Is<MyPupilsMessage>(message => message.Equals(messageStub))), Times.Once);
    }

    [Fact]
    public void AddMessage_Appends_To_Existing_Messages()
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        Mock<ITempDataDictionary> tempDataMock = new();
        string? capturedJson = null;

        tempDataMock.Setup(
            (tempData) =>
                tempData.Peek(options.Value.MessagesKey))
                    .Returns(MESSAGES_SERIALISED_STUB);

        tempDataMock
            .SetupSet(tempData => tempData[options.Value.MessagesKey] = It.IsAny<object>())
            .Callback<string, object>((key, value) =>
            {
                capturedJson = value as string;
            });

        Mock<ITempDataDictionaryProvider> providerMock = new();
        providerMock.Setup(t => t.GetTempData()).Returns(tempDataMock.Object);

        Mock<IMapper<MyPupilsMessage, MyPupilsMessageDto>> toDtoMapper =
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>();

        toDtoMapper
            .Setup(t => t.Map(It.IsAny<MyPupilsMessage>()))
            .Returns<MyPupilsMessage>(
                (message) => new MyPupilsMessageDto()
                {
                    Id = message.Id,
                    Message = message.Message,
                    MessageLevel = message.Level
                });

        MyPupilsTempDataMessageSink sut = new(
            toDtoMapper.Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            options,
            providerMock.Object);

        // Act
        MyPupilsMessage addMessageStub = MyPupilsMessage.Create("id", MessageLevel.Debug, "test");
        sut.AddMessage(addMessageStub);

        // Assert

        List<MyPupilsMessageDto> expectedMessages = [
            ..JsonConvert.DeserializeObject<List<MyPupilsMessageDto>>(MESSAGES_SERIALISED_STUB),
            new()
            {
                Id = addMessageStub.Id,
                MessageLevel = MessageLevel.Debug,
                Message = addMessageStub.Message
            },
        ];

        Assert.False(string.IsNullOrWhiteSpace(capturedJson));

        List<MyPupilsMessageDto>? messages =
            JsonConvert.DeserializeObject<List<MyPupilsMessageDto>>(capturedJson!);

        Assert.NotNull(messages);
        Assert.Equivalent(expectedMessages, messages);

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);
        toDtoMapper.Verify(t => t.Map(It.Is<MyPupilsMessage>(message => message.Equals(addMessageStub))), Times.Once);
    }

    public static TheoryData<object> AddMessageObjectsInTempDataInputs
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
}