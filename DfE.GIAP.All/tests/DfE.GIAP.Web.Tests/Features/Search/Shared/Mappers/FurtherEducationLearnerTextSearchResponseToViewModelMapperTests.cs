using Bogus;
using DfE.GIAP.Core.Common.Application.ValueObjects;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Search.Facets;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.Models;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation.SearchByName;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;
using DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Features.Search.Shared.Mappers;

public class FurtherEducationLearnerTextSearchResponseToViewModelMapperTests
{
    [Fact]
    public void Constructor_WithNullLearnerMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationLearnerTextSearchResponseToViewModelMapper(
                null, FiltersMapperTestDouble.Mock().Object));
    }

    [Fact]
    public void Constructor_WithNullFiltersMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new FurtherEducationLearnerTextSearchResponseToViewModelMapper(
                FurtherEducationLearnerMapperToLearnerTestDoubles.Mock().Object, null));
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

        FurtherEducationSearchByNameResponse response =
            FurtherEducationSearchByNameResponseTestDouble.Create(
                learners: new FurtherEducationLearners([applicationModelLearners[0]]),
                facets: SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 1),
                totalResults: 1);

        List<FilterData> filters = [new() { Name = "Region" }];

        Mock<IMapper<SearchFacets, List<FilterData>>> filtersMapper =
            FiltersMapperTestDouble.MockFor(response.FacetedResults!, filters);

        Mock<IMapper<FurtherEducationLearner, Learner>> learnerMapper =
            FurtherEducationLearnerMapperToLearnerTestDoubles.MockForMultiple(new[]
            {
                (applicationModelLearners[0], domainLearners[0]),
                (applicationModelLearners[1], domainLearners[1])
            });

        LearnerTextSearchViewModel model = new()
        {
            PageSize = 1
        };

        FurtherEducationLearnerTextSearchMappingContext context =
            FurtherEducationLearnerTextSearchMappingContext.Create(model, response);

        FurtherEducationLearnerTextSearchResponseToViewModelMapper mapper = new(learnerMapper.Object, filtersMapper.Object);

        // act
        LearnerTextSearchViewModel result = mapper.Map(context);

        // assert
        Learner mappedLearner = result.Learners.Single();
        Assert.Equal(applicationModelLearners[0].Identifier.UniqueLearnerNumber.ToString().Trim(), mappedLearner.Id);
        Assert.Equal(filters, result.Filters);
        Assert.Equal(1, result.Count);
        Assert.Equal(1, result.Total);
        Assert.False(result.ShowOverLimitMessage);
    }
}
