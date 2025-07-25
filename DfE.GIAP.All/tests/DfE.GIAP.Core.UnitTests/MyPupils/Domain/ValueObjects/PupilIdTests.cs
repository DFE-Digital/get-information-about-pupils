using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.ValueObjects;

public sealed class PupilIdTests
{
    [Fact]
    public void Id_ShouldReturnStringRepresentationOfGuid()
    {
        // Arrange
        const string input = "12345678-1234-1234-1234-1234567890ab";
        PupilId pupilId = new(Guid.Parse(input));

        // Act Assert
        Assert.Equal(input, pupilId.Id);
    }

    [Fact]
    public void Equality_ShouldBeBasedOnGuidValue()
    {
        // Arrange
        Guid guid = Guid.NewGuid();
        PupilId pupilId1 = new(guid);
        PupilId pupilId2 = new(guid);

        // Act Assert
        Assert.Equal(pupilId1, pupilId2);
        Assert.True(pupilId1.Equals(pupilId2));
    }

    [Fact]
    public void Inequality_ShouldBeTrue_ForDifferentGuids()
    {
        // Arrange
        PupilId pupilId1 = new(Guid.NewGuid());
        PupilId pupilId2 = new(Guid.NewGuid());

        // Act Assert
        Assert.NotEqual(pupilId1, pupilId2);
    }
}
