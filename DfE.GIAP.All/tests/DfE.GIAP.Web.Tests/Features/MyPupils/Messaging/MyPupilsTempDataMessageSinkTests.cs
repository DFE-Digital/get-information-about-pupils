using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Shared.TempData;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
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
    private List<MyPupilsMessageDto> CreateExpectedMessageDtos(string json) => JsonConvert.DeserializeObject<List<MyPupilsMessageDto>>(json)!;

    [Fact]
    public void Constructor_Throws_When_ToDataTransferObjectMapper_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            null!,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            new Mock<ITempDataDictionaryProvider>().Object,
            new Mock<IJsonSerializer>().Object);

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
            new Mock<ITempDataDictionaryProvider>().Object,
            new Mock<IJsonSerializer>().Object);

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
            new Mock<ITempDataDictionaryProvider>().Object,
            new Mock<IJsonSerializer>().Object);

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
            new Mock<ITempDataDictionaryProvider>().Object,
            new Mock<IJsonSerializer>().Object);

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
            null!,
            new Mock<IJsonSerializer>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_JsonSerializer_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            new Mock<ITempDataDictionaryProvider>().Object,
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

        Mock<IJsonSerializer> jsonSerializer = new();

        MyPupilsTempDataMessageSink sut = new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            options,
            providerMock.Object,
            jsonSerializer.Object);

        // Act
        IReadOnlyList<MyPupilsMessage> response = sut.GetMessages();

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response);

        tempDataMock.Verify(tempData => tempData[options.Value.MessagesKey], Times.Once);

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);

        jsonSerializer.Verify(serializer => serializer.Deserialize<List<MyPupilsMessageDto>>(It.IsAny<string>()), Times.Never);
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

        List<MyPupilsMessageDto>? messageDtoStubs = CreateExpectedMessageDtos(MESSAGES_SERIALISED_STUB);

        Mock<IJsonSerializer> jsonSerializerMock = new();
        List<MyPupilsMessageDto>? capturedMessageDtos = [];

        jsonSerializerMock
            .Setup(s => s.TryDeserialize(It.IsAny<string>(), out messageDtoStubs))
            .Callback((string _, out List<MyPupilsMessageDto>? outArg) =>
            {
                // Capture what serializer was called with
                outArg = messageDtoStubs;
                capturedMessageDtos = outArg;
            })
            .Returns(true);

        MyPupilsTempDataMessageSink sut = new(
            MapperTestDoubles.Default<MyPupilsMessage, MyPupilsMessageDto>().Object,
            mapperFromDtoMock.Object,
            options,
            providerMock.Object,
            jsonSerializerMock.Object);

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

        tempDataMock.Verify(tempData => tempData[options.Value.MessagesKey], Times.Once);

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);

        mapperFromDtoMock.Verify(
            mapper => mapper.Map(It.IsAny<MyPupilsMessageDto>()), Times.Exactly(3));

        jsonSerializerMock.Verify(
            s => s.TryDeserialize<List<MyPupilsMessageDto>>(
                It.Is<string>(t => t == MESSAGES_SERIALISED_STUB),
                out It.Ref<List<MyPupilsMessageDto>?>.IsAny),
            Times.Once);

        Assert.Same(messageDtoStubs, capturedMessageDtos);

    }

    [Theory]
    [MemberData(nameof(AddMessageObjectsInTempDataInputs))]
    public void AddMessage_Appends_To_NullOrEmpty_Messages(object? stored)
    {
        // Arrange
        IOptions<MyPupilsMessagingOptions> options =
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>();

        const string serialisedMessagesJsonStub = @"Test json";
        string? capturedJson = null;

        Mock<ITempDataDictionary> tempDataMock = new();

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

        Mock<IJsonSerializer> jsonSerializer = new();
        object? capturedSerializeObject = null;
        jsonSerializer
            .Setup((serialiser) => serialiser.Serialize(It.IsAny<object>()))
            .Callback((object arg) => capturedSerializeObject = arg)
            .Returns(serialisedMessagesJsonStub);

        MyPupilsTempDataMessageSink sut = new(
            mapperMock.Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            options,
            providerMock.Object,
            jsonSerializer.Object);

        // Act
        MyPupilsMessage messageStub = new(MessageLevel.Debug, "test");

        // Act
        sut.AddMessage(messageStub);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(capturedJson));

        providerMock.Verify(provider => provider.GetTempData(), Times.Once);

        mapperMock.Verify(t => t.Map(It.Is<MyPupilsMessage>(message => message.Equals(messageStub))), Times.Once);

        // nothing to deserialise as empty
        jsonSerializer.Verify(serializer => serializer.Deserialize<List<MyPupilsMessageDto>>(It.IsAny<string>()), Times.Never);

        MyPupilsMessageDto actualMessage =
            Assert.IsType<IEnumerable<MyPupilsMessageDto>>(capturedSerializeObject, exactMatch: false)
                .Single();

        Assert.Equal(messageStub.Id, actualMessage.Id);
        Assert.Equal(messageStub.Level, actualMessage.MessageLevel);
        Assert.Equal(messageStub.Message, actualMessage.Message);

        tempDataMock.VerifySet(tempData => tempData[options.Value.MessagesKey] = serialisedMessagesJsonStub);
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

        Mock<IJsonSerializer> jsonSerializerMock = new();

        const string serialisedMessagesJsonStub = @"Test json";

        object? capturedSerializedMessages = null;
        jsonSerializerMock
            .Setup((serialiser) => serialiser.Serialize(It.IsAny<object>()))
            .Callback((object arg) => capturedSerializedMessages = arg)
            .Returns(serialisedMessagesJsonStub);


        List<MyPupilsMessageDto>? existingMessages = CreateExpectedMessageDtos(MESSAGES_SERIALISED_STUB);
        List<MyPupilsMessageDto>? capturedExistingMessages = null;
        jsonSerializerMock
            .Setup(s => s.TryDeserialize(It.IsAny<string>(), out capturedExistingMessages))
            .Callback((string _, out List<MyPupilsMessageDto>? outArg) =>
            {
                // Capture what serializer was called with
                outArg = existingMessages;
                capturedExistingMessages = outArg;
            })
            .Returns(true);

        MyPupilsTempDataMessageSink sut = new(
            mapperMock.Object,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            options,
            providerMock.Object,
            jsonSerializerMock.Object);

        // Act
        MyPupilsMessage addMessageStub = MyPupilsMessage.Create("id", MessageLevel.Debug, "test");
        sut.AddMessage(addMessageStub);

        // Assert
        providerMock.Verify(provider => provider.GetTempData(), Times.Once);

        tempDataMock.Verify(tempData => tempData.Peek(options.Value.MessagesKey), Times.Once);

        mapperMock.Verify(
            (mapper) => mapper.Map(
                It.Is<MyPupilsMessage>(
                    (message) => message.Equals(addMessageStub))), Times.Once);

        jsonSerializerMock.Verify(
            s => s.TryDeserialize(
                It.Is<string>(t => t == MESSAGES_SERIALISED_STUB),
                out It.Ref<List<MyPupilsMessageDto>?>.IsAny),
            Times.Once);

        jsonSerializerMock.Verify((serializer) => serializer.Serialize(It.IsAny<object>()), Times.Once);

        jsonSerializerMock.Verify(
            s => s.TryDeserialize(
                It.Is<string>(t => t == MESSAGES_SERIALISED_STUB),
                out It.Ref<List<MyPupilsMessageDto>?>.IsAny),
            Times.Once);

        Assert.Equivalent(existingMessages, Assert.IsType<IEnumerable<MyPupilsMessageDto>>(capturedExistingMessages, exactMatch: false).ToList());

        List<MyPupilsMessageDto> expectedToSerialiseMessages = [
           ..existingMessages,
            new()
            {
                Id = addMessageStub.Id,
                MessageLevel = MessageLevel.Debug,
                Message = addMessageStub.Message
            },
        ];
        Assert.Equivalent(expectedToSerialiseMessages, capturedSerializedMessages);

        tempDataMock.VerifySet(tempData => tempData[options.Value.MessagesKey] = serialisedMessagesJsonStub);
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
