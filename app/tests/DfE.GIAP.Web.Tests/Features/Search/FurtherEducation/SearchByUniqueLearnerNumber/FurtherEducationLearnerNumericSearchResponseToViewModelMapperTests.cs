using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByUniqueLearnerNumber;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;
using DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.SearchByUniqueLearnerNumber;

public class FurtherEducationLearnerNumericSearchResponseToViewModelMapperTests
{
    [Fact]
    public void Constructor_WithNullLearnerMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationLearnerNumericSearchResponseToViewModelMapper(furtherEducationLearnerToViewModelMapper: null));
    }

    [Fact]
    public void Map_WithValidContext_MapsLearnersAndFiltersCorrectly()
    {
        // arrange
        Faker faker = new();
        (string Uln, string FirstName, string Surname, DateTime BirthDate, Sex Gender)[] learnerFakes =
        [
            FakeFurtherEducationLearnerDataTestDouble.CreateFurtherEducationLearnerFake(faker),
            FakeFurtherEducationLearnerDataTestDouble.CreateFurtherEducationLearnerFake(faker)
        ];

        List<Learner> domainLearners = LearnerTestDouble.CreateLearnersStub(learnerFakes);
        List<FurtherEducationLearner> applicationModelLearners =
            FurtherEducationLearnerCollectionTestDouble.CreateLearnersStub(learnerFakes);

        SearchResponse<FurtherEducationLearners> response =
            SearchResponse<FurtherEducationLearners>.Create(
                FurtherEducationLearners.Create([applicationModelLearners[0]]),
                totalResults: 1);

        Mock<IMapper<FurtherEducationLearner, Learner>> learnerMapper =
            FurtherEducationLearnerMapperToLearnerTestDoubles.MockForMultiple(new[]
            {
                (applicationModelLearners[0], domainLearners[0]),
                (applicationModelLearners[1], domainLearners[1])
            });

        LearnerNumberSearchViewModel model = new()
        {
            PageSize = 1
        };

        FurtherEducationLearnerNumericSearchMappingContext context =
            FurtherEducationLearnerNumericSearchMappingContext.Create(model, response);

        FurtherEducationLearnerNumericSearchResponseToViewModelMapper mapper = new(learnerMapper.Object);

        // act
        LearnerNumberSearchViewModel result = mapper.Map(context);

        // assert
        Learner mappedLearner = result.Learners.Single();
        Assert.Equal(applicationModelLearners[0].Identifier.UniqueLearnerNumber.ToString().Trim(), mappedLearner.Id);
        Assert.Equal(1, result.Total);
    }
}
