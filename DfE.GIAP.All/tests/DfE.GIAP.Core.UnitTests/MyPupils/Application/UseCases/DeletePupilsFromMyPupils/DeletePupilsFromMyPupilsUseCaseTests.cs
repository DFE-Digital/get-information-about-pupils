using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
public sealed class DeletePupilsFromMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        Func<DeletePupilsFromMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_WriteRepository_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        Func<DeletePupilsFromMyPupilsUseCase> construct = () => new(readRepoMock.Object, null!, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<DeletePupilsFromMyPupilsUseCase> construct = () => new(readRepoMock.Object, writeRepoMock.Object, null!);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleRequestAsync_Request_Is_Null_Throws()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock =
            IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(stub: null!);

        Mock<IMyPupilsWriteOnlyRepository> writeRepositoryMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        DeletePupilsFromMyPupilsUseCase sut = new(
            readRepositoryMock.Object,
            writeRepositoryMock.Object,
            mapperMock.Object);

        // Act Assert
        Func<Task> act = async () => await sut.HandleRequestAsync(request: null!);
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData("\n")]
    [InlineData(null)]
    public async Task HandleRequestAsync_NullOrWhitespaceUserId_Throws(string? userId)
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock =
            IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(stub: null!);

        Mock<IMyPupilsWriteOnlyRepository> writeRepositoryMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        DeletePupilsFromMyPupilsUseCase sut = new(
            readRepositoryMock.Object,
            writeRepositoryMock.Object,
            mapperMock.Object);

        DeletePupilsFromMyPupilsRequest request = new(userId!, DeletePupilUpns: []);

        // Act Assert

        Func<Task> act = async () => await sut.HandleRequestAsync(request);
        await Assert.ThrowsAnyAsync<ArgumentException>(act);
    }

    [Fact]
    public async Task HandleRequestAsync_ReadRepository_Returns_Null_Returns()
    {
        // Arrange
        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock =
            IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(stub: null!);

        Mock<IMyPupilsWriteOnlyRepository> writeRepositoryMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        DeletePupilsFromMyPupilsUseCase sut = new(
            readRepositoryMock.Object,
            writeRepositoryMock.Object,
            mapperMock.Object);

        const string userId = "id";
        DeletePupilsFromMyPupilsRequest request = new(
            UserId: userId,
            DeletePupilUpns: []);

        // Act
        await sut.HandleRequestAsync(request);

        // Assert
        readRepositoryMock
            .Verify(
                (readRepo) => readRepo.GetMyPupilsOrDefaultAsync(
                    It.Is<MyPupilsId>(t => t.Value == userId)), Times.Once);

        writeRepositoryMock
            .Verify(
                (writeRepo) => writeRepo.SaveMyPupilsAsync(
                    It.IsAny<MyPupilsAggregate>()), Times.Never);

        mapperMock.Verify(
            (mapper) => mapper.Map(
                It.IsAny<IEnumerable<string>>()), Times.Never);
    }

    [Fact]
    public async Task HandleRequestAsync_WhenNoUpnsMatch_ThrowsArgumentException()
    {
        // Arrange
        UniquePupilNumbers aggregateUpns =
            UniquePupilNumbers.Create(uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 3));

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Create(aggregateUpns);

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock =
            IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupils);

        Mock<IMyPupilsWriteOnlyRepository> mockWriteRepository = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        DeletePupilsFromMyPupilsUseCase useCase = new(
            readRepositoryMock.Object, mockWriteRepository.Object, mapperMock.Object);

        List<string> deletePupilUpnsDoNotExistInAggregate =
            UniquePupilNumberTestDoubles.Generate(count: 1)
                .Select(t => t.Value)
                .ToList();

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: myPupils.AggregateId.Value,
            DeletePupilUpns: deletePupilUpnsDoNotExistInAggregate);

        // Act 
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleRequestAsync(request));

        // Assert
        readRepositoryMock.Verify(
            (repo) => repo.GetMyPupilsOrDefaultAsync(
                myPupils.AggregateId), Times.Once);

        mockWriteRepository.Verify(repo =>
            repo.SaveMyPupilsAsync(myPupils), Times.Never);
    }

    [Fact]
    public async Task HandleRequestAsync_Maps_SomeDeletePupilsUpns_DeletePupils_From_MyPupilsAggregate_And_Calls_WriteRepository()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsAggregate myPupilsAggregate =
            MyPupilsAggregateTestDoubles.Create(
                myPupilsId,
                uniquePupilNumbers: UniquePupilNumbers.Create(
                                        UniquePupilNumberTestDoubles.Generate(count: 10)));

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock =
            IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupilsAggregate);

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock =
            IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        mapperMock
            .Setup(t => t.Map(It.IsAny<IEnumerable<string>>()))
            .Returns(
                UniquePupilNumbers.Create(myPupilsAggregate.GetMyPupils()));

        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, writeRepoMock.Object, mapperMock.Object);

        List<string> deletePupilIdentifiers =
            myPupilsAggregate.GetMyPupils()
                .Select(t => t.Value)
                .Take(2)
                .ToList();

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: myPupilsId.Value,
            DeletePupilUpns: deletePupilIdentifiers);

        // Act
        await useCase.HandleRequestAsync(request);

        // Assert
        readRepositoryMock.Verify(
            (readRepo) => readRepo.GetMyPupilsOrDefaultAsync(myPupilsId),
            Times.Once);

        writeRepoMock.Verify(writeRepo =>
            writeRepo.SaveMyPupilsAsync(
                It.Is<MyPupilsAggregate>(
                    (myPupils)
                        => !myPupils.GetMyPupils() // Removed all of the deletePupilIdentifiers
                                .Select(t => t.Value)
                                .Any(deletePupilIdentifiers.Contains))),
            Times.Once);

        mapperMock.Verify(
            (mapper) => mapper.Map(
                It.Is<IEnumerable<string>>((upns) => upns.SequenceEqual(deletePupilIdentifiers))), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_Maps_DeletePupilUpns_Then_DeletesPupils_From_MyPupilsAggregate_And_Calls_WriteRepository()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsAggregate myPupils =
            MyPupilsAggregateTestDoubles.Create(
                myPupilsId,
                uniquePupilNumbers: UniquePupilNumbers.Create(
                                        UniquePupilNumberTestDoubles.Generate(count: 10)));

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock =
            IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupils);

        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock =
            IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Mock<IMapper<IEnumerable<string>, UniquePupilNumbers>> mapperMock =
            MapperTestDoubles.Default<IEnumerable<string>, UniquePupilNumbers>();

        mapperMock
            .Setup(t => t.Map(It.IsAny<IEnumerable<string>>()))
            .Returns(
                UniquePupilNumbers.Create(myPupils.GetMyPupils()));

        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, writeRepoMock.Object, mapperMock.Object);

        List<string> deletePupilIdentifiers =
            myPupils.GetMyPupils()
                .Select(t => t.Value)
                .ToList();

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: myPupilsId.Value,
            DeletePupilUpns: deletePupilIdentifiers);

        // Act
        await useCase.HandleRequestAsync(request);

        // Assert
        readRepositoryMock.Verify(
            (readRepo) => readRepo.GetMyPupilsOrDefaultAsync(myPupilsId),
            Times.Once);

        writeRepoMock.Verify(writeRepo =>
            writeRepo.SaveMyPupilsAsync(
                It.Is<MyPupilsAggregate>((myPupils) => !myPupils.GetMyPupils().Any())),
            Times.Once);

        mapperMock.Verify(
            (mapper) => mapper.Map(
                It.Is<IEnumerable<string>>((upns) => upns.SequenceEqual(deletePupilIdentifiers))), Times.Once);
    }
}
