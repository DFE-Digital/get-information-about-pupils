using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;
public sealed class UniquePupilNumberTests
{
    public static TheoryData<string> ValidUpnValues => new()
    {
        "A12345678901X", // A + 11 digits + X
        "T98765432109Z", // T + 11 digits + Z
        "B00000000000A", // B + 11 digits + A
        "H11111111111C", // H + 11 digits + C
        "J12345678901X", // J + 11 digits + X
        "N12345678901Y", // N + 11 digits + Y
        "P12345678901Z", // P + 11 digits + Z
        "R123456789012", // R + 12 digits
        "W12345678901X", // W is allowed
        "Z12345678901X",  // Z is allowed
        "U12345678901X", // U is allowed
        "V12345678901X", // V is allowed
        "Q12345678901X", // Q is allowed
    };

    public static TheoryData<string?> InvalidUpnValues => new()
    {
        "I12345678901X", // Invalid prefix (I not allowed)
        "O12345678901X", // Invalid prefix (O not allowed)
        "S12345678901X", // Invalid prefix (S not allowed)
        "A1234567890X",   // Too short (10 digits + letter)
        "A1234567890123", // Too long (13 digits)
        "A12345678901",   // Missing final char (only 11 digits)
        "A12345678901!",  // Invalid final char (non-alphanumeric)
        "",               // Empty
        "   ",            // Whitespace
        "\r\n",           // Windows new line
        "   \r\n ",       // Windows new line with whitespace
        "\n",             // Unix new line
        null              // Null
    };


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
