using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

namespace DfE.GIAP.Core.UnitTests.Common.TextSanitiser;
public sealed class TextSanitiserHandlerTests
{
    [Fact]
    public void Handle_WithNoCustomSanitisers_AppliesDefaultHtmlSanitiser()
    {
        // Arrange
        TextSanitisationInvoker handler = new(null!);

        // Act
        SanitisedTextResult result = handler.Handle("<script onClick=evil()>Hello</script>");

        // Assert
        Assert.DoesNotContain("script", result.Value);
    }

    [Fact]
    public void Handle_WithCustomSanitisers_AppliesAllInOrder()
    {
        // Arrange
        FakeTextToUpperCaseSanitiser testUpperCaseSanitiser = new();
        FakeMaliciousScriptSanitiser maliciousScriptSanitiser = new();
        TextSanitisationInvoker handler = new(sanitisers: [testUpperCaseSanitiser, maliciousScriptSanitiser]);

        // Act
        SanitisedTextResult result = handler.Handle("hello");

        // Assert
        Assert.Equal("HELLO", result.Value);
        Assert.Equal(1, testUpperCaseSanitiser.ExecutionCount);
        Assert.Equal(1, maliciousScriptSanitiser.ExecutionCount);
    }

    internal sealed class FakeTextToUpperCaseSanitiser : ITextSanitiserHandler
    {
        public int ExecutionCount { get; private set; }
        public SanitisedText Sanitise(string raw)
        {
            ExecutionCount++;
            return new(raw.ToUpper());
        }
    }

    internal sealed class FakeMaliciousScriptSanitiser : ITextSanitiserHandler
    {
        public int ExecutionCount { get; private set; }

        public SanitisedText Sanitise(string raw)
        {
            ExecutionCount++;
            // Simulated malicious input for testing purposes only
            string malicious = raw + "<script>alert('xss');</script>";
            return new(malicious);
        }
    }
}
