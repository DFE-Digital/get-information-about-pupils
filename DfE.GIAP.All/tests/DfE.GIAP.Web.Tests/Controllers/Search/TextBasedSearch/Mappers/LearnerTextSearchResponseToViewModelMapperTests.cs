using Bogus;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Application.Models.Learner;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;
using DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Moq;
using Xunit;
using static DfE.GIAP.Core.Search.Application.Models.Learner.LearnerCharacteristics;

namespace DfE.GIAP.Web.Tests.Controllers.TextBasedSearch.Mappers;

public class LearnerTextSearchResponseToViewModelMapperTests
{
    [Fact]
    public void Constructor_WithNullLearnerMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new LearnerTextSearchResponseToViewModelMapper(
                null, FiltersMapperTestDouble.Mock().Object));
    }

    [Fact]
    public void Constructor_WithNullFiltersMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new LearnerTextSearchResponseToViewModelMapper(
                LearnerMapperTestDouble.Mock().Object, null));
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
        List<Core.Search.Application.Models.Learner.Learner> applicationModelLearners =
            ApplicationModelLearnersTestDouble.CreateLearnersStub(learnerFakes);

        SearchResponse response =
            SearchByKeyWordsResponseTestDouble.Create(
                learners: new Learners([applicationModelLearners[0]]),
                facets: SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 1),
                status: SearchResponseStatus.Success,
                totalResults: 1);

        List<FilterData> filters = [new() { Name = "Region" }];

        Mock<IMapper<SearchFacets, List<FilterData>>> filtersMapper =
            FiltersMapperTestDouble.MockFor(response.FacetedResults!, filters);

        Mock<IMapper<Core.Search.Application.Models.Learner.Learner, Domain.Search.Learner.Learner>> learnerMapper =
            LearnerMapperTestDouble.MockForMultiple(new[]
            {
                (applicationModelLearners[0], domainLearners[0]),
                (applicationModelLearners[1], domainLearners[1])
            });

        LearnerTextSearchViewModel model = new() { MaximumResults = 10 };
        LearnerTextSearchResponseToViewModelMapper.LearnerTextSearchMappingContext context =
            LearnerTextSearchResponseToViewModelMapper.LearnerTextSearchMappingContext.Create(model, response);

        LearnerTextSearchResponseToViewModelMapper mapper = new(learnerMapper.Object, filtersMapper.Object);

        // act
        LearnerTextSearchViewModel result = mapper.Map(context);

        // assert
        Domain.Search.Learner.Learner mappedLearner = result.Learners.Single();
        Assert.Equal(applicationModelLearners[0].Identifier.UniqueLearnerNumber.ToString().Trim(), mappedLearner.Id);
        Assert.Equal(filters, result.Filters);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.Total);
        Assert.False(result.ShowOverLimitMessage);
    }
}
