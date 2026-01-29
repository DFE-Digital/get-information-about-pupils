using DfE.GIAP.Core.Common.Domain;
using DfE.GIAP.Core.MyPupils.Application.Mapper;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Mapper;
public sealed class UniquePupilNumberMapperTests
{
    [Fact]
    public void Map_Null_Returns_Empty()
    {
        // Arrange
        UniquePupilNumbersMapper sut = new();

        // Act
        UniquePupilNumbers response = sut.Map(null!);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.GetUniquePupilNumbers());
    }

    [Fact]
    public void Map_Empty_Returns_Empty()
    {
        // Arrange
        UniquePupilNumbersMapper sut = new();

        // Act
        UniquePupilNumbers response = sut.Map([]);

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.GetUniquePupilNumbers());
    }

    [Fact]
    public void Map_Removes_Invalid_Or_Empty_And_Returns_Valid_Upns()
    {
        // Arrange
        UniquePupilNumbersMapper sut = new();

        // Act
        List<string> validUpns = UniquePupilNumberTestDoubles.GenerateAsValues(count: 2);
        UniquePupilNumbers response = sut.Map([validUpns[0], string.Empty, "invalid", validUpns[1], " ", "\n"]);

        // Assert
        Assert.NotNull(response);
        Assert.Equivalent(validUpns, response.GetUniquePupilNumbers().Select(t => t.Value));
    }
}
