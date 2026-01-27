using Bogus;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Learner.FurtherEducation;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Response;
using DfE.GIAP.Web.Controllers.LearnerNumber.Mappers;
using DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Moq;
using Xunit;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Web.Tests.Controllers.Search.LearnerNumber.Mappers;

public class LearnerNumericSearchResponseToViewModelMapperTests
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
        (string Uln, string FirstName, string Surname, DateTime BirthDate, Gender Gender)[] learnerFakes =
        [
            FakeLearnerDataTestDouble.CreateLearnerFake(faker),
            FakeLearnerDataTestDouble.CreateLearnerFake(faker)
        ];

        List<Domain.Search.Learner.Learner> domainLearners = DomainLearnersTestDouble.CreateLearnersStub(learnerFakes);
        List<FurtherEducationLearner> applicationModelLearners =
            ApplicationModelLearnersTestDouble.CreateLearnersStub(learnerFakes);

        FurtherEducationSearchResponse response =
            SearchByKeyWordsResponseTestDouble.Create(
                learners: new FurtherEducationLearners([applicationModelLearners[0]]),
                facets: SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 1),
                status: SearchResponseStatus.Success,
                totalResults: 1);

        Mock<IMapper<FurtherEducationLearner, Domain.Search.Learner.Learner>> learnerMapper =
            LearnerMapperTestDouble.MockForMultiple(new[]
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
        Domain.Search.Learner.Learner mappedLearner = result.Learners.Single();
        Assert.Equal(applicationModelLearners[0].Identifier.UniqueLearnerNumber.ToString().Trim(), mappedLearner.Id);
        Assert.Equal(1, result.Total);
    }
}
