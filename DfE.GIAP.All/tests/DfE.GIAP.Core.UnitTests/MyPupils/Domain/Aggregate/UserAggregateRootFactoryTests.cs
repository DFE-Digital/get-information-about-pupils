using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Repository;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.SharedTests.TestDoubles;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Domain.Aggregate;


public sealed class UserAggregateRootFactoryTests
{

    [Fact]
    public void Constructor_ThrowsException_When_UserRepository_Is_Null()
    {
        // Assert
        Mock<IAggregatePupilsForMyPupilsDomainService> mockAggregatePupilsService = new();
        Mock<IMapper<IAuthorisationContext, PupilAuthorisationContext>> mockMapper = MapperTestDoubles.Default<IAuthorisationContext, PupilAuthorisationContext>();

        // Act
        Func<UserAggregateRootFactory> act = () =>
            new UserAggregateRootFactory(
                null!,
                mockAggregatePupilsService.Object,
                mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_ThrowsException_When_AggregatePupilService_Is_Null()
    {
        Mock<IUserReadOnlyRepository> mockUserReadOnlyRepository = new();
        Mock<IMapper<IAuthorisationContext, PupilAuthorisationContext>> mockMapper = MapperTestDoubles.Default<IAuthorisationContext, PupilAuthorisationContext>();

        // Act
        Func<UserAggregateRootFactory> act = () =>
            new UserAggregateRootFactory(
                mockUserReadOnlyRepository.Object,
                null!,
                mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_ThrowsException_When_Mapper_Is_Null()
    {
        // Assert
        Mock<IUserReadOnlyRepository> mockUserReadOnlyRepository = new();
        Mock<IAggregatePupilsForMyPupilsDomainService> mockAggregatePupilsService = new();

        // Act
        Func<UserAggregateRootFactory> act = () =>
            new UserAggregateRootFactory(
                mockUserReadOnlyRepository.Object,
                mockAggregatePupilsService.Object,
                null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public async Task CreateAsync_ReturnsUserAggregateRoot_WithExpectedPupils()
    {
        // Arrange

        string userIdString = "my-user-id";
        Mock<IAuthorisationContext> authorisationContext = new();
        authorisationContext.Setup(t => t.UserId).Returns(userIdString);
        IAuthorisationContext context = authorisationContext.Object;

        UserId userId = new(userIdString);
        PupilAuthorisationContext pupilAuthorisationContext = PupilAuthorisationContextTestDoubles.Default();

        Mock<IMapper<IAuthorisationContext, PupilAuthorisationContext>> mockMapper = new();
        mockMapper.Setup(m => m.Map(It.IsAny<IAuthorisationContext>()))
            .Returns(pupilAuthorisationContext);

        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(3);
                User user = new(userId, upns);

        Mock<IUserReadOnlyRepository> mockUserReadOnlyRepository = new();
        mockUserReadOnlyRepository.Setup(repo => repo.GetUserByIdAsync(
                It.Is<UserId>(t => t.Equals(userId)),
                It.IsAny<IAuthorisationContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user)
            .Verifiable();

        Pupil stubPupil1 = PupilBuilder.CreateBuilder(upns[0], pupilAuthorisationContext).Build();
        Pupil stubPupil2 = PupilBuilder.CreateBuilder(upns[1], pupilAuthorisationContext).Build();
        Pupil stubPupil3 = PupilBuilder.CreateBuilder(upns[2], pupilAuthorisationContext).Build();

        Mock<IAggregatePupilsForMyPupilsDomainService> aggregatePupilsService = new();
        aggregatePupilsService.Setup(
            (t) => t.GetPupilsAsync(
                It.Is<IEnumerable<UniquePupilNumber>>(
                    (upns) => upns.Count() == 3),
                    pupilAuthorisationContext))
            .ReturnsAsync([stubPupil1, stubPupil2, stubPupil3])
            .Verifiable();

        // Act
        UserAggregateRootFactory userAggregateRootFactory = new(
            mockUserReadOnlyRepository.Object,
            aggregatePupilsService.Object,
            mockMapper.Object);

        UserAggregateRoot result = await userAggregateRootFactory.CreateAsync(context);

        // Assert
        
        Assert.NotNull(result);
        Assert.Equal(userId, result.Identifier);

        List<Pupil> pupils = result.GetMyPupils().ToList();
        Assert.Equal(3, pupils.Count);
        Assert.Equivalent(pupils, new[] { stubPupil1, stubPupil2, stubPupil3 } );
    }
}
