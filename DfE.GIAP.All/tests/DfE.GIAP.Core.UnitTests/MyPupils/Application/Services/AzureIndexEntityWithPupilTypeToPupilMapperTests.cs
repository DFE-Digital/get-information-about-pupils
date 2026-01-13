using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Mapper;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;
public sealed class AzureIndexEntityWithPupilTypeToPupilMapperTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        AzureIndexEntityWithPupilTypeToPupilMapper mapper = new();

        // Act Assert
        Func<Pupil> act = () => mapper.Map(null!);
        Assert.Throws<ArgumentNullException>(act);
    }
}
