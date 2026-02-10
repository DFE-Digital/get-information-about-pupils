using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Web.Tests.Features.Search.PupilPremium;
using DfE.GIAP.Web.Tests.Features.Search.Shared.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;
public sealed class NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapperTests
{

    [Fact]
    public void Constructor_WithNullLearnerMapper_ThrowsArgumentNullException()
    {
        // act/assert
        Assert.Throws<ArgumentNullException>(() =>
            new NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapper(null!));
    }

    [Fact]
    public void Map_WithValidContext_MapsLearnersAndFiltersCorrectly()
    {
        // arrange
        List<Learner> domainLearners = [
            LearnerFakePupilPremiumTestDoubles.Fake(),
            LearnerFakePupilPremiumTestDoubles.Fake()
        ];

        List<NationalPupilDatabaseLearner> learners = NationalPupilDatabaseLearnerTestDoubles.FakeMany(domainLearners.Count);

        NationalPupilDatabaseSearchByUniquePupilNumberResponse response =
            NationalPupilDatabaseSearchByUniquePupilNumberResponseTestDouble.Create(
                learners: new NationalPupilDatabaseLearners(learners),
                totalResults: learners.Count);

        Mock<IMapper<NationalPupilDatabaseLearner, Learner>> learnerMapper = new();
        learnerMapper
            .Setup((mapper) => mapper.Map(It.Is<NationalPupilDatabaseLearner>(t => t == learners[0])))
            .Returns(domainLearners[0]);
        learnerMapper
            .Setup((mapper) => mapper.Map(It.Is<NationalPupilDatabaseLearner>(t => t == learners[1])))
            .Returns(domainLearners[1]);

        LearnerNumberSearchViewModel model = new()
        {
            PageSize = 1
        };


        NationalPupilDatabaseLearnerNumericSearchMappingContextToViewModelMapper mapper = new(learnerMapper.Object);

        // act
        LearnerNumberSearchViewModel result = mapper.Map(
            NationalPupilDatabaseLearnerNumericSearchMappingContext.Create(
                model, response));

        // assert
        Assert.Equivalent(domainLearners, result.Learners);
        Assert.Equal(2, result.Total);

        learnerMapper.Verify(t => t.Map(It.Is<NationalPupilDatabaseLearner>(req => ReferenceEquals(req, learners[0]))), Times.Once());
        learnerMapper.Verify(t => t.Map(It.Is<NationalPupilDatabaseLearner>(req => ReferenceEquals(req, learners[1]))), Times.Once());
    }
}
