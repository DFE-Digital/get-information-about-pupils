using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Infrastructure.Repositories;

public sealed class FurtherEducationPupilDtoToEntityMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        FurtherEducationPupilDtoToEntityMapper mapper = new();

        // Act
        Action act = () => mapper.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsAllPropertiesCorrectly()
    {
        // Arrange
        FurtherEducationPupilDto inputDto = FurtherEducationPupilDtoTestDoubles.Generate(count: 1).Single();
        FurtherEducationPupilDtoToEntityMapper mapper = new();

        // Act
        FurtherEducationPupil result = mapper.Map(inputDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(inputDto.UniqueLearnerNumber, result.UniqueLearnerNumber);
        Assert.Equal(inputDto.Forename, result.Forename);
        Assert.Equal(inputDto.Surname, result.Surname);
        Assert.Equal(inputDto.Gender, result.Gender);
        Assert.Equal(inputDto.DOB, result.DOB);
        Assert.Equal(inputDto.ConcatenatedName, result.ConcatenatedName);

        Assert.NotNull(result.PupilPremium);
        Assert.Equal(inputDto.PupilPremium[0].NationalCurriculumYear, result.PupilPremium[0].NationalCurriculumYear);
        Assert.Equal(inputDto.PupilPremium[0].FullTimeEquivalent, result.PupilPremium[0].FullTimeEquivalent);
        Assert.Equal(inputDto.PupilPremium[0].AcademicYear, result.PupilPremium[0].AcademicYear);

        Assert.NotNull(result.specialEducationalNeeds);
        Assert.Equal(inputDto.specialEducationalNeeds[0].NationalCurriculumYear, result.specialEducationalNeeds[0].NationalCurriculumYear);
        Assert.Equal(inputDto.specialEducationalNeeds[0].Provision, result.specialEducationalNeeds[0].Provision);
        Assert.Equal(inputDto.specialEducationalNeeds[0].AcademicYear, result.specialEducationalNeeds[0].AcademicYear);
    }
}
