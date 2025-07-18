using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public sealed class UniquePupilNumberTests
{
    public static TheoryData<string> ValidUpnValues => [
            "A12345678901X" , // A
            "T98765432109Z" , // T
            "A00000000000A" , // A Any 11 digits between 
            "T11111111111B" // T Any 11 digits between
        ];

    public static TheoryData<string> InvalidUpnValues => [
            "B12345678901X" , // Invalid prefix
            "A1234567890X" ,  // Too short
            "A1234567890123" , // Too long
            "A12345678901" ,  // Missing check digit
            "A12345678901!" , // Invalid check digit
            "",              // Empty
            "   " ,           // Whitespace
            "\r\n",          // Windows new line
            "   \r\n " ,      // Windows new line with whitespace
            "\n" ,            // Unix new line
            null!              // Null
        ];

    [Theory]
    [MemberData(nameof(ValidUpnValues))]
    public void IsValidUpn_ReturnsTrue_ForValidUpns(string upn)
    {
        // Act
        UniquePupilNumber constructed = new(upn);

        // Assert
        Assert.Equal(upn, constructed.Value);
    }

    [Theory]
    [MemberData(nameof(InvalidUpnValues))]
    public void IsValidUpn_ReturnsFalse_ForInvalidUpns(string? upn)
    {
        // Act
        Func<UniquePupilNumber> construct = () => new UniquePupilNumber(upn!);

        // Assert
        Assert.Throws<ArgumentException>(construct);
    }


    [Theory]
    [MemberData(nameof(ValidUpnValues))]
    public void TryCreate_ReturnsTrueAndConstructsUpn_ForValidUpns(string upn)
    {
        // Act
        bool result = UniquePupilNumber.TryCreate(upn, out UniquePupilNumber? constructed);

        // Assert
        Assert.True(result);
        Assert.NotNull(constructed);
        Assert.Equal(upn, constructed!.Value);
    }

    [Theory]
    [MemberData(nameof(InvalidUpnValues))]
    public void TryCreate_ReturnsFalseAndNull_ForInvalidUpns(string? upn)
    {
        // Act
        bool result = UniquePupilNumber.TryCreate(upn, out UniquePupilNumber? constructed);

        // Assert
        Assert.False(result);
        Assert.Null(constructed);
    }


    [Fact]
    public void ToString_ReturnsValue()
    {
        // Arrange
        string input = UniquePupilNumberTestDoubles.Generate().Value;
        UniquePupilNumber upn = new(input);

        // Act
        string result = upn.ToString();

        // Assert
        Assert.Equal(input, result);
    }

    [Fact]
    public void Equality_ShouldBeBasedOnValue()
    {
        // Arrange
        string input = UniquePupilNumberTestDoubles.Generate().Value;
        UniquePupilNumber upn1 = new(input);
        UniquePupilNumber upn2 = new(input);

        // Act Assert
        Assert.Equal(upn1, upn2);
        Assert.True(upn1.Equals(upn2));
    }

    [Fact]
    public void Inequality_ShouldBeTrue_ForDifferentValues()
    {
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(2);
        UniquePupilNumber upn1 = new(upns[0].Value);
        UniquePupilNumber upn2 = new(upns[1].Value);

        // Act Assert
        Assert.NotEqual(upn1, upn2);
        Assert.False(upn1.Equals(upn2));
    }
}
