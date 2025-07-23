using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
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
                    pupilAuthorisationContext,
                    It.IsAny<PupilQuery>()))
            .ReturnsAsync(stubPupils)
            .Verifiable();

        // Act
        UserAggregateRootFactory aggregateRootFactory = new(
            mockUserReadOnlyRepository.Object,
            aggregatePupilsService.Object,
            mockMapper.Object);

        Mock<IAuthorisationContext> authorisationContext = AuthorisationContextTestDoubles.MockFor(userIdInput);
        UserAggregateRoot result = await aggregateRootFactory.CreateAsync(authorisationContext.Object, It.IsAny<PupilQuery>());

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.Identifier);

        List<PupilDto> pupilDtos = result.GetMyPupils().ToList();
        Assert.Equal(3, stubPupils.Count());

        IEnumerable<(Pupil Entity, PupilDto Dto)> entityToDto = stubPupils.Zip(pupilDtos, (pupil, dto) => (pupil, dto));

        Assert.All(entityToDto, item =>
        {
            Assert.Equal(item.Entity.Identifier.Id, item.Dto.Id);
            Assert.Equal(item.Entity.Forename, item.Dto.Forename);
            Assert.Equal(item.Entity.Surname, item.Dto.Surname);
            Assert.Equal(item.Entity.Sex, item.Dto.Sex);
            Assert.Equal(item.Entity.UniquePupilNumber, item.Dto.UniquePupilNumber);
            Assert.Equal(item.Entity.DateOfBirth, item.Dto.DateOfBirth);
            Assert.Equal(item.Entity.LocalAuthorityCode, item.Dto.LocalAuthorityCode);
        });
    }
}
