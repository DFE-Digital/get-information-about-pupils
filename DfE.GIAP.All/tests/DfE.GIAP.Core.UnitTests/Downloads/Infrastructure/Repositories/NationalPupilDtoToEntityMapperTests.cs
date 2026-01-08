using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories;

public sealed class NationalPupilDtoToEntityMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        NationalPupilDtoToEntityMapper mapper = new();

        // Act
        Action act = () => mapper.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsAllPropertiesCorrectly()
    {
        // Arrange
        NationalPupilDto inputDto = NationalPupilDtoTestDoubles.Generate(count: 1).Single();
        NationalPupilDtoToEntityMapper mapper = new();

        // Act
        NationalPupil result = mapper.Map(inputDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(inputDto.Upn, result.Upn);
        Assert.Equal(inputDto.Forename, result.Forename);
        Assert.Equal(inputDto.Surname, result.Surname);
        Assert.Equal(inputDto.Gender, result.Gender);
        Assert.Equal(inputDto.DOB, result.DOB);

        Assert.NotNull(result.CensusSummer);
        Assert.NotNull(result.CensusAutumn);
        Assert.NotNull(result.CensusSpring);
        Assert.NotNull(result.KeyStage1);
        Assert.NotNull(result.KeyStage2);
        Assert.NotNull(result.KeyStage4);
        Assert.NotNull(result.Phonics);
        Assert.NotNull(result.MTC);
        Assert.NotNull(result.EarlyYearsFoundationStageProfile);
    }
}
