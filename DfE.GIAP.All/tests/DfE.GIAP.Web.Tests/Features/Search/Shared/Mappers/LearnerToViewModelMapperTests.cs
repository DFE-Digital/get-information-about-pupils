using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Web.Features.Search.FurtherEducation;
using DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Mappers;

public sealed class LearnerToViewModelMapperTests
{
    private readonly FurtherEducationLearnerToLearnerMapper _mapper = new();

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
            FurtherEducationLearnerTestDouble.Stub(
                uniqueLearnerNumber: "1234567890",
                firstname: "Alice",
                surname: "Smith",
                birthDate: new DateTime(2000, 1, 15),
                sex: Sex.Female);

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

