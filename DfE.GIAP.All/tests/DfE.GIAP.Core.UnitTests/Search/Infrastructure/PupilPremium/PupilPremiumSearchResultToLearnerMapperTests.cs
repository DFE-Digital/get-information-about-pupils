using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.DataTransferObjects;
using DfE.GIAP.Core.Search.Infrastructure.PupilPremium.Mappers;
using DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.PupilPremium;

public sealed class PupilPremiumSearchResultToLearnerMapperTests
{
    private readonly PupilPremiumLearnerDataTransferObjectToPupilPremiumLearnerMapper _sut;

    private readonly PupilPremiumLearnerDataTransferObject _mappingInput;
    public PupilPremiumSearchResultToLearnerMapperTests()
    {
        _sut = new();

        _mappingInput = PupilPremiumLearnerDataTransferObjectTestDoubles.Stub();
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Map(null!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ThrowsArgumentException_WhenUPNIsNullOrEmpty(string? upn)
    {
        // Arrange
        _mappingInput.UPN = upn;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => _sut.Map(_mappingInput));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ThrowsArgumentException_WhenForenameIsNullOrEmpty(string? forename)
    {
        // Arrange
        _mappingInput.Forename = forename;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => _sut.Map(_mappingInput));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Map_ThrowsArgumentException_WhenSurnameIsNullOrEmpty(string? surname)
    {
        // Arrange
        _mappingInput.Surname = surname;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => _sut.Map(_mappingInput));
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenDOBIsNull()
    {
        // Arrange
        _mappingInput.DOB = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Map(_mappingInput));
    }

    [Fact]
    public void Map_ThrowsArgumentNullException_WhenLocalAuthorityIsNull()
    {
        // Arrange
        _mappingInput.LocalAuthority = null!;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _sut.Map(_mappingInput));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Map_ThrowsArgumentException_WhenLocalAuthorityIsEmptyOrWhiteSpace(string localAuthority)
    {
        // Arrange
        _mappingInput.LocalAuthority = localAuthority;

        // Act & Assert
        Assert.ThrowsAny<ArgumentException>(() => _sut.Map(_mappingInput));
    }

    [Fact]
    public void Map_ReturnsLearner_WhenInputIsValid()
    {
        // Act
        PupilPremiumLearner result = _sut.Map(_mappingInput);

        // Assert
        Assert.NotNull(result);
    }

    [Fact]
    public void Map_SetsEmptyMiddleNames_WhenMiddleNamesIsNull()
    {
        // Arrange
        _mappingInput.Middlenames = null;

        // Act
        PupilPremiumLearner result = _sut.Map(_mappingInput);

        // Assert
        // Adjust this assertion to match your domain model API if needed.
        Assert.Equal(string.Empty, result.Name.MiddleNames);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("m")]
    [InlineData(" M ")]
    public void Map_MapsSexFromSex_WhenSexIsProvided_Male(string sex)
    {
        // Arrange
        _mappingInput.Sex = sex;
        _mappingInput.Gender = "F"; // Should be ignored because Sex is not null/whitespace

        // Act
        PupilPremiumLearner result = _sut.Map(_mappingInput);

        // Assert
        Assert.Equal(Sex.Male, result.Characteristics.Sex);
    }

    [Theory]
    [InlineData("F")]
    [InlineData("f")]
    [InlineData(" F ")]
    public void Map_MapsSexFromSex_Female_WhenSexIsProvided_Female(string sex)
    {
        // Arrange
        _mappingInput.Sex = sex;
        _mappingInput.Gender = "M"; // Should be ignored because Sex is not null/whitespace

        // Act
        PupilPremiumLearner result = _sut.Map(_mappingInput);

        // Assert
        Assert.Equal(Sex.Female, result.Characteristics.Sex);
    }

    [Theory]
    [InlineData("O")]
    [InlineData("o")]
    [InlineData(" z ")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("X")]
    [InlineData("Unknown")]
    public void Map_MapsSexFromSex_Unknown_WhenSexIsNullOrUnknown(string? sex)
    {
        // Arrange
        _mappingInput.Sex = sex;
        _mappingInput.Gender = "F"; // Should be ignored because Sex is not null/whitespace

        // Act
        PupilPremiumLearner result = _sut.Map(_mappingInput);

        // Assert
        Assert.Equal(Sex.Unknown, result.Characteristics.Sex);
    }
}
