using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.NationalPupilDatabase.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.NationalPupilDatabase;
public sealed class NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.Map(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ThrowsArgumentException_WhenUPNIsNullOrEmpty(string? upn)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.UPN = upn;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => mapper.Map(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ThrowsArgumentException_WhenForenameIsNullOrEmpty(string? forename)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.Forename = forename;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => mapper.Map(input));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ThrowsArgumentException_WhenSurnameIsNullOrEmpty(string? surname)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.Surname = surname;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => mapper.Map(input));
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenDOBIsNull()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.DOB = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.Map(input));
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenLocalAuthorityIsNull()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.LocalAuthority = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => mapper.Map(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Map_ThrowsArgumentException_WhenLocalAuthorityIsEmptyOrWhiteSpace(string localAuthority)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.LocalAuthority = localAuthority;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => mapper.Map(input));
    }

    [Fact]
    public void Map_ReturnsLearner_WhenInputIsValid()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();

        // Act
        NationalPupilDatabaseLearner result = mapper.Map(input);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Map_SetsEmptyMiddleNames_WhenMiddleNamesIsNull()
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.Middlenames = null;

        // Act
        NationalPupilDatabaseLearner result = mapper.Map(input);

        // Assert
        // Adjust this assertion to match your domain model API if needed.
        Assert.Equal(string.Empty, result.Name.MiddleNames);
    }

    [Theory]
    [InlineData("M", Gender.Male)]
    [InlineData("m", Gender.Male)]
    [InlineData(" M ", Gender.Male)]
    [InlineData("F", Gender.Female)]
    [InlineData("f", Gender.Female)]
    [InlineData(" F ", Gender.Female)]
    [InlineData("O", Gender.Other)]
    [InlineData("o", Gender.Other)]
    [InlineData(" O ", Gender.Other)]
    public void Map_MapsGenderFromSex_WhenSexIsProvided(string sex, Gender expected)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.Sex = sex;
        input.Gender = "F"; // Should be ignored because Sex is not null/whitespace

        // Act
        NationalPupilDatabaseLearner result = mapper.Map(input);

        // Assert
        Assert.Equal(expected, result.Characteristics.Sex);
    }

    [Theory]
    [InlineData("M", Gender.Male)]
    [InlineData(" F ", Gender.Female)]
    [InlineData("o", Gender.Other)]
    public void Map_MapsGenderFromGender_WhenSexIsNullOrWhiteSpace(string gender, Gender expected)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.Sex = "   "; // triggers fallback to Gender
        input.Gender = gender;

        // Act
        NationalPupilDatabaseLearner result = mapper.Map(input);

        // Assert
        Assert.Equal(expected, result.Characteristics.Sex);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    [InlineData("Unknown")]
    public void Map_DefaultsGenderToOther_WhenSexAndGenderAreUnrecognisedOrMissing(string? value)
    {
        // Arrange
        NationalPupilDatabaseLearnerDataTransferObjectToNationalPupilDatabaseLearnerMapper mapper = new();
        NationalPupilDatabaseLearnerDataTransferObject input = NationalPupilDatabaseLearnerDataTransferObjectTestDoubles.Stub();
        input.Sex = value;
        input.Gender = value;

        // Act
        NationalPupilDatabaseLearner result = mapper.Map(input);

        // Assert
        Assert.Equal(Gender.Other, result.Characteristics.Sex);
    }
}
