using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase;
public sealed class NationalPupilDatabaseLearnerToLearnerMapperTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        NationalPupilDatabaseLearnerToLearnerMapper mapper = new();
        Func<Learner> act = () => mapper.Map(null!);

        // Act & Assert
        Assert.ThrowsAny<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Maps_All_Expected_Fields()
    {
        // Arrange
        NationalPupilDatabaseLearnerToLearnerMapper mapper = new();

        NationalPupilDatabaseLearner input = NationalPupilDatabaseLearnerTestDoubles.Fake();

        // Act
        Learner result = mapper.Map(input);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(input.Identifier.Value, result.Id);
        Assert.Equal(input.Identifier.Value, result.LearnerNumber);

        Assert.Equal(input.Name.FirstName, result.Forename);
        Assert.Equal(input.Name.MiddleNames, result.Middlenames);
        Assert.Equal(input.Name.Surname, result.Surname);

        Assert.Equal(input.Characteristics.Sex.ToString(), result.Sex);

        Assert.Equal(input.Characteristics.BirthDate, result.DOB);
        Assert.Equal(input.LocalAuthority.Code.ToString(), result.LocalAuthority);
    }

    [Fact]
    public void Map_Maps_Empty_MiddleNames_When_Input_Has_Empty_MiddleNames()
    {
        // Arrange
        NationalPupilDatabaseLearnerToLearnerMapper mapper = new();

        NationalPupilDatabaseLearner input = NationalPupilDatabaseLearnerTestDoubles.FakeWithMiddleName(null);

        // Act
        Learner result = mapper.Map(input);

        // Assert
        Assert.Equal(string.Empty, result.Middlenames);
    }
}
