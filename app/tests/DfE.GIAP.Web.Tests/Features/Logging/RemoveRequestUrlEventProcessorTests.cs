using DfE.GIAP.Web.Features.Logging.Sentry;
using Sentry;

namespace DfE.GIAP.Web.Tests.Features.Logging;

public class RemoveRequestUrlEventProcessorTests
{
    [Fact]
    public void Process_RemovesRequestUrlAndQueryString()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        sentryEvent.Request.Url = "https://example.test/search?upn=1234567890123";
        sentryEvent.Request.QueryString = "?upn=1234567890123";

        // Act
        SentryEvent result = new RemoveRequestUrlEventProcessor().Process(sentryEvent);

        // Assert
        Assert.Same(sentryEvent, result);
        Assert.Null(result.Request.Url);
        Assert.Null(result.Request.QueryString);
    }

    [Fact]
    public void Process_RemovesRequestUrlContainingPupilIdentifierWithoutQueryString()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        sentryEvent.Request.Url = "https://example.test/pupils/1234567890123";

        // Act
        SentryEvent result = new RemoveRequestUrlEventProcessor().Process(sentryEvent);

        // Assert
        Assert.Null(result.Request.Url);
        Assert.Null(result.Request.QueryString);
    }

    [Fact]
    public void Process_RemovesQueryStringContainingSearchTermsWithoutRequestUrl()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        sentryEvent.Request.QueryString = "?forename=Jane&surname=Smith";

        // Act
        SentryEvent result = new RemoveRequestUrlEventProcessor().Process(sentryEvent);

        // Assert
        Assert.Null(result.Request.Url);
        Assert.Null(result.Request.QueryString);
    }

    [Fact]
    public void Process_PreservesOtherRequestDetailsWhenUrlFieldsAreAbsent()
    {
        // Arrange
        SentryEvent sentryEvent = new();
        sentryEvent.Request.Method = "POST";

        // Act
        SentryEvent result = new RemoveRequestUrlEventProcessor().Process(sentryEvent);

        // Assert
        Assert.Same(sentryEvent, result);
        Assert.Equal("POST", result.Request.Method);
        Assert.Null(result.Request.Url);
        Assert.Null(result.Request.QueryString);
    }
}
