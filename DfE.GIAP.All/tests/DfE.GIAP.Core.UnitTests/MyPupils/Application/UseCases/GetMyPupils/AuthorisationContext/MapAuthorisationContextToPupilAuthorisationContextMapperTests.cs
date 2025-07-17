using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
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

        MapAuthorisationContextToPupilsAuthorisationContextMapper mapper = new();

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
        MapAuthorisationContextToPupilsAuthorisationContextMapper mapper = new();

        Func<PupilAuthorisationContext> act = () => mapper.Map(null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(act);
    }

}
