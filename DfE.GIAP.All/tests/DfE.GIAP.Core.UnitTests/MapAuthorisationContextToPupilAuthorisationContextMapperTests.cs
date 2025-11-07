namespace DfE.GIAP.Core.UnitTests;

public sealed class MapAuthorisationContextToPupilAuthorisationContextMapperTests
{

    [Theory]
    [InlineData(4, 11, false)]
    [InlineData(10, 18, true)]
    public void Map_WithVariousInputs_MapsCorrectly(int lowAge, int highAge, bool isAdmin)
    {
        // Arrange
        Mock<IAuthorisationContext> mockContext = new();
        mockContext.SetupGet(c => c.LowAge).Returns(lowAge);
        mockContext.SetupGet(c => c.HighAge).Returns(highAge);
        mockContext.SetupGet(c => c.IsAdministrator).Returns(isAdmin);

        MapAuthorisationContextToMyPupilsAuthorisationContextMapper mapper = new();

        // Act
        PupilAuthorisationContext result = mapper.Map(mockContext.Object);

        // Assert
        PupilAuthorisationContext expected = new(
            new AgeLimit(lowAge, highAge),
            new UserRole(isAdmin));

        Assert.Equal(expected, result);
    }

    [Fact]
    public void Map_WithNullInput_ThrowsArgumentNullException()
    {
        // Arrange
        MapAuthorisationContextToMyPupilsAuthorisationContextMapper mapper = new();

        Func<PupilAuthorisationContext> act = () => mapper.Map(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

}
