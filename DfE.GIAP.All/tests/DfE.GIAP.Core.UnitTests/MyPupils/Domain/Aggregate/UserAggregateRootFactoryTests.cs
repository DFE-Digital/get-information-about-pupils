using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.AuthorisationContext;
using DfE.GIAP.Core.MyPupils.Domain.Aggregate;
using DfE.GIAP.Core.MyPupils.Domain.Authorisation;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.Services;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.Core.UnitTests.User.TestDoubles;
using DfE.GIAP.Core.User.Application.Repository;

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
        // Arrange
        IUserReadOnlyRepository mockUserReadOnlyRepository = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<IAuthorisationContext, PupilAuthorisationContext>> mockMapper = MapperTestDoubles.Default<IAuthorisationContext, PupilAuthorisationContext>();

        // Act
        Func<UserAggregateRootFactory> act = () =>
            new UserAggregateRootFactory(
                mockUserReadOnlyRepository,
                null!,
                mockMapper.Object);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Constructor_ThrowsException_When_Mapper_Is_Null()
    {
        // Assert
        IUserReadOnlyRepository mockUserReadOnlyRepository = UserReadOnlyRepositoryTestDoubles.Default();
        Mock<IAggregatePupilsForMyPupilsDomainService> mockAggregatePupilsService = new();

        // Act
        Func<UserAggregateRootFactory> act = () =>
            new UserAggregateRootFactory(
                mockUserReadOnlyRepository,
                mockAggregatePupilsService.Object,
                null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public async Task CreateAsync_ReturnsUserAggregateRoot_WithExpectedPupils()
    {
        // Arrange

        string userIdInput = "my-user-id";

        UserId userId = new(userIdInput);
        PupilAuthorisationContext pupilAuthorisationContext = PupilAuthorisationContextTestDoubles.Default();

        Mock<IMapper<IAuthorisationContext, PupilAuthorisationContext>> mockMapper = MapperTestDoubles.MockFor<IAuthorisationContext, PupilAuthorisationContext>(pupilAuthorisationContext);

        const int pupilCount = 3;
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(pupilCount);
        Core.User.Application.Repository.User user = new(userId, upns);

        Mock<IUserReadOnlyRepository> mockUserReadOnlyRepository = UserReadOnlyRepositoryTestDoubles.MockForGetUserById(user);

        IEnumerable<Pupil> stubPupils = [
            PupilBuilder.CreateBuilder(upns[0], pupilAuthorisationContext).Build(),
            PupilBuilder.CreateBuilder(upns[1], pupilAuthorisationContext).Build(),
            PupilBuilder.CreateBuilder(upns[2], pupilAuthorisationContext).Build()
        ];

        Mock<IAggregatePupilsForMyPupilsDomainService> aggregatePupilsService = new();
        aggregatePupilsService.Setup((t)
            => t.GetPupilsAsync(
                    It.Is<IEnumerable<UniquePupilNumber>>((upns) => upns.Count() == pupilCount),
                    pupilAuthorisationContext))
            .ReturnsAsync(stubPupils)
            .Verifiable();

        // Act
        UserAggregateRootFactory userAggregateRootFactory = new(
            mockUserReadOnlyRepository.Object,
            aggregatePupilsService.Object,
            mockMapper.Object);

        Mock<IAuthorisationContext> authorisationContext = AuthorisationContextTestDoubles.MockFor(userIdInput);
        UserAggregateRoot result = await userAggregateRootFactory.CreateAsync(authorisationContext.Object);

        // Assert

        Assert.NotNull(result);
        Assert.Equal(userId, result.Identifier);

        List<Pupil> pupils = result.GetMyPupils().ToList();
        Assert.Equal(3, stubPupils.Count());
        Assert.Equivalent(stubPupils, pupils);
    }
}
