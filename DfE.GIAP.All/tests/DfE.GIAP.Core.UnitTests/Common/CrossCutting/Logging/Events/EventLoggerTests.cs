using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Models;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events.Sinks;

namespace DfE.GIAP.Core.UnitTests.Common.CrossCutting.Logging.Events;

public class EventLoggerTests
{
    private readonly Mock<IBusinessEventFactory> _businessEventFactoryMock;
    private readonly Mock<IEventSink> _sinkMock;
    private readonly EventLogger _sut;

    public EventLoggerTests()
    {
        _businessEventFactoryMock = new Mock<IBusinessEventFactory>();
        _sinkMock = new Mock<IEventSink>();
        IEnumerable<IEventSink> sinks = new List<IEventSink> { _sinkMock.Object };

        _sut = new EventLogger(_businessEventFactoryMock.Object, sinks);
    }

    [Fact]
    public void Constructor_BusinessEventFactoryIsNull_Should_ThrowArgumentNullException()
    {
        IEnumerable<IEventSink> sinks = new List<IEventSink> { _sinkMock.Object };

        Assert.Throws<ArgumentNullException>(() => new EventLogger(null!, sinks));
    }

    [Fact]
    public void Constructor_SinksIsNull_Should_ThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EventLogger(_businessEventFactoryMock.Object, null!));
    }

    [Fact]
    public void LogSearch_ShouldCallDispatch_WhichInvokesSinkLog()
    {
        // Arrange
        SearchEvent fakeEvent = new("user", "session", "desc", "urn", "org", "cat",
            new SearchPayload(SearchIdentifierType.UPN, true, new Dictionary<string, bool>()));
        _businessEventFactoryMock
            .Setup(f => f.CreateSearch(SearchIdentifierType.UPN, true, It.IsAny<Dictionary<string, bool>>()))
            .Returns(fakeEvent);

        // Act
        _sut.LogSearch(SearchIdentifierType.UPN, true, new Dictionary<string, bool>());

        // Assert
        _sinkMock.Verify(s => s.Log(fakeEvent), Times.Once);
    }

    [Fact]
    public void LogDownload_ShouldCallDispatch_WhichInvokesSinkLog()
    {
        // Arrange
        DownloadEvent fakeEvent = new("user", "session", "desc", "urn", "org", "cat",
            new DownloadPayload(DownloadType.Search, DownloadFileFormat.CSV, DownloadEventType.NPD, "batch", Dataset.KS1));
        _businessEventFactoryMock
            .Setup(f => f.CreateDownload(DownloadType.Search, DownloadFileFormat.CSV, DownloadEventType.NPD, "batch", Dataset.KS1))
            .Returns(fakeEvent);

        // Act
        _sut.LogDownload(DownloadType.Search, DownloadFileFormat.CSV, DownloadEventType.NPD, "batch", Dataset.KS1);

        // Assert
        _sinkMock.Verify(s => s.Log(fakeEvent), Times.Once);
    }

    [Fact]
    public void LogSignin_ShouldCallDispatch_WhichInvokesSinkLog()
    {
        // Arrange
        SigninEvent fakeEvent = new("user", "session", "desc", "urn", "org", "cat", new SigninPayload());
        _businessEventFactoryMock
            .Setup(f => f.CreateSignin("user", "session", "urn", "org", "cat"))
            .Returns(fakeEvent);

        // Act
        _sut.LogSignin("user", "session", "urn", "org", "cat");

        // Assert
        _sinkMock.Verify(s => s.Log(fakeEvent), Times.Once);
    }
}
