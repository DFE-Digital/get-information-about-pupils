using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Web.Features.Search.FurtherEducation;
using DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers;

public sealed class LearnerToViewModelMapperTests
{
    private readonly FurtherEducationLearnerToViewModelMapper _mapper = new();

    [Fact]
    public void Map_WithNullInput_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() => _mapper.Map(null!));
    }

    [Fact]
    public void Map_WithValidLearner_MapsAllFieldsCorrectly()
    {
        // arrange
        FurtherEducationLearner applicationModellearner =
            ApplicationModelLearnerTestDouble.Stub(
                uniqueLearnerNumber: "1234567890",
                firstname: "Alice",
                surname: "Smith",
                birthDate: new DateTime(2000, 1, 15),
                sex: Gender.Female);

        // act
        Domain.Search.Learner.Learner result = _mapper.Map(applicationModellearner);

        // assert
        Assert.Equal("1234567890", result.Id);
        Assert.Equal("1234567890", result.LearnerNumber);
        Assert.Equal("Alice", result.Forename);
        Assert.Equal("Smith", result.Surname);
        Assert.Equal("F", result.Sex);
        Assert.Equal(new DateTime(2000, 1, 15), result.DOB);
    }
}

