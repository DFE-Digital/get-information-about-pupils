using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Sanitiser;

namespace DfE.GIAP.Core.UnitTests.Common.TextSanitiser;

public sealed class HtmlTextSanitiserTests
{
    private readonly HtmlTextSanitiser _sanitiser;

    public HtmlTextSanitiserTests()
    {
        _sanitiser = new();
    }

    [Fact]
    public void Sanitise_RemovesScriptTags()
    {
        // Arrange
        string input = "<script>alert('xss');</script><p>Hello</p>";

        // Act
        SanitisedText result = _sanitiser.Sanitise(input);

        // Assert
        Assert.Equal("<p>Hello</p>", result.Value);
        Assert.DoesNotContain("<script>", result.Value, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Sanitise_AllowsClassAttribute()
    {
        // Arrange
        string input = "<p class='highlight'>Text</p>";

        // Act
        SanitisedText result = _sanitiser.Sanitise(input);

        // Assert
        Assert.Equal("<p class=\"highlight\">Text</p>", result.Value);
    }

    [Fact]
    public void Sanitise_RemovesDisallowedAttributes()
    {
        // Arrange
        string input = "<p onclick='evil()'>Click me</p>";

        // Act
        SanitisedText result = _sanitiser.Sanitise(input);

        // Assert
        Assert.DoesNotContain("onclick", result.Value);
        Assert.Equal("<p>Click me</p>", result.Value);
    }

    [Theory]
    [InlineData("\r\n")]
    [InlineData("     \r   \n ")]
    public void Sanitise_Strips_CarriageReturns(string input)
    {
        // Act
        SanitisedText result = _sanitiser.Sanitise(input!);

        // Assert
        Assert.DoesNotContain("\r", result.Value);
        Assert.Contains("\n", result.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\n")]
    public void Sanitise_HandlesEmptyOrNull_Input(string? input)
    {
        // Act
        SanitisedText result = _sanitiser.Sanitise(input!);

        // Assert
        string expected = input ?? string.Empty;
        Assert.Equal(expected, result.Value);
    }
}
