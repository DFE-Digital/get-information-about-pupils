using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium.Models;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Tests.Features.Search.PupilPremium.TestDoubles;
using DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.SearchByUniquePupilNumber;
public sealed class PupilPremiumLearnerNumericSearchMappingContextTests
{

    [Fact]
    public void Constructor_WithNullLearnerMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new PupilPremiumLearnerNumericSearchMappingContextToViewModelMapper(pupilPremiumModelToLearnerMapper: null));
    }

    [Fact]
    public void Map_WithValidContext_MapsLearnersAndFiltersCorrectly()
    {
        // arrange
        List<Learner> domainLearners = [
            LearnerFakePupilPremiumTestDoubles.Fake(),
            LearnerFakePupilPremiumTestDoubles.Fake()
        ];

        List<PupilPremiumLearner> pupilPremiumLearners = PupilPremiumLearnerTestDoubles.FakeMany(domainLearners.Count);

        PupilPremiumSearchResponse response =
            PupilPremiumSearchResponseTestDouble.Create(
                learners: new PupilPremiumLearners(pupilPremiumLearners),
                facets: SearchFacetsTestDouble.CreateSingleFacetGroup("Region", "North", 1),
                status: SearchResponseStatus.Success,
                totalResults: pupilPremiumLearners.Count);

        Mock<IMapper<PupilPremiumLearner, Learner>> learnerMapper = new();
        learnerMapper
            .Setup((mapper) => mapper.Map(It.Is<PupilPremiumLearner>(t => t == pupilPremiumLearners[0])))
            .Returns(domainLearners[0]);
        learnerMapper
            .Setup((mapper) => mapper.Map(It.Is<PupilPremiumLearner>(t => t == pupilPremiumLearners[1])))
            .Returns(domainLearners[1]);

        LearnerNumberSearchViewModel model = new()
        {
            PageSize = 1
        };


        PupilPremiumLearnerNumericSearchMappingContextToViewModelMapper mapper = new(learnerMapper.Object);

        // act
        LearnerNumberSearchViewModel result = mapper.Map(
            PupilPremiumLearnerNumericSearchMappingContext.Create(
                model, response));

        // assert
        Assert.Equivalent(domainLearners, result.Learners);
        Assert.Equal(2, result.Total);

        learnerMapper.Verify(t => t.Map(It.Is<PupilPremiumLearner>(req => ReferenceEquals(req, pupilPremiumLearners[0]))), Times.Once());
        learnerMapper.Verify(t => t.Map(It.Is<PupilPremiumLearner>(req => ReferenceEquals(req, pupilPremiumLearners[1]))), Times.Once());
    }
}
