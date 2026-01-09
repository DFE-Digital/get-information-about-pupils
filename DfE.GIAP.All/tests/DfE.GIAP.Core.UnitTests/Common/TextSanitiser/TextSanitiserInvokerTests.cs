using System.Text;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;

namespace DfE.GIAP.Core.UnitTests.Common.TextSanitiser;
public sealed class TextSanitiserInvokerTests
{

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Sanitise_With_NullOrEmpty_DoesNotThrow(string? input)
    {
        Core.Common.Application.TextSanitiser.Invoker.TextSanitiser handler = new(null!);

        SanitisedTextResult output = handler.Sanitise(input!);

        Assert.Equal(string.Empty, output.Value);
    }

    [Fact]
    public void Sanitise_WithNoSanitisers_Returns_Result()
    {
        // Arrange
        const string result = "<script onClick=evil()>Hello</script>";
        Core.Common.Application.TextSanitiser.Invoker.TextSanitiser handler = new(null!);

        // Act
        SanitisedTextResult output = handler.Sanitise(result);

        // Assert
        Assert.Equal(result, output.Value);
    }

    [Fact]
    public void Sanitise_WithCustomSanitisers_AppliesAll()
    {
        // Arrange
        FakeTextToUpperCaseSanitiser upperCaseSanitiser = new();
        FakeRemoveCharacterSanitiser removeCharacterSanitiser = new('L');
        Core.Common.Application.TextSanitiser.Invoker.TextSanitiser handler = new(sanitisers: [upperCaseSanitiser, removeCharacterSanitiser]);

        // Act
        SanitisedTextResult result = handler.Sanitise("hello");

        // Assert
        Assert.Equal("HEO", result.Value);
        Assert.Equal(1, upperCaseSanitiser.ExecutionCount);
        Assert.Equal(1, removeCharacterSanitiser.ExecutionCount);
    }

    internal sealed class FakeTextToUpperCaseSanitiser : ITextSanitiserHandler
    {
        public int ExecutionCount { get; private set; }
        public SanitisedText Handle(string? raw)
        {
            ExecutionCount++;
            return new(raw!.ToUpper());
        }
    }

    internal sealed class FakeRemoveCharacterSanitiser : ITextSanitiserHandler
    {
        private readonly char _characterToSanitise;

        public FakeRemoveCharacterSanitiser(char characterToSanitise)
        {
            _characterToSanitise = characterToSanitise;
        }

        public int ExecutionCount { get; private set; }

        public SanitisedText Handle(string? raw)
        {
            ExecutionCount++;
            StringBuilder builder = new();
            foreach (char c in raw!)
            {
                if (c != _characterToSanitise)
                {
                    builder.Append(c);
                }
            }
            return new(builder.ToString());
        }
    }
}
