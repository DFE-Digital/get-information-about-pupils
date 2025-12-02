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
    //[Fact]
    //public async Task HandleRequestAsync_WhenDeleteAllIsTrue_SavesEmptyList()
    //{
    //    // Arrange
    //    MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

    //    MyPupilsAggregate myPupils =
    //        MyPupilsTestDoubles.Create(
    //            myPupilsId,
    //            UniquePupilNumbers.Create(
    //                UniquePupilNumberTestDoubles.Generate(count: 10)));

    //    Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupils);
    //    Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
    //    DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, writeRepoMock.Object);

    //    DeletePupilsFromMyPupilsRequest request = new(
    //        UserId: myPupilsId.Value,
    //        DeletePupilUpns: myPupils.GetMyPupils().Select(t => t.Value)); // these should be ignored when deleteAll toggled

    //    // Act
    //    await useCase.HandleRequestAsync(request);

    //    // Assert
    //    readRepositoryMock.Verify(
    //        (readRepo) => readRepo.GetMyPupilsOrDefaultAsync(
    //            myPupilsId,
    //            It.IsAny<CancellationToken>()),
    //        Times.Once);

    //    writeRepoMock.Verify(writeRepo =>
    //        writeRepo.SaveMyPupilsAsync(
    //            myPupilsId,
    //            It.Is<MyPupilsAggregate>(myPupils => !myPupils.GetMyPupils().Any()),
    //            It.IsAny<CancellationToken>()),
    //        Times.Once);
    //}

    [Fact]
    public async Task HandleRequestAsync_WhenSomeUpnsAreValid_SavesUpnsThatAreNotDeleted()
    {
        // Arrange
        UniquePupilNumbers upns =
            UniquePupilNumbers.Create(
                uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 3));

        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Create(myPupilsId, upns);

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(myPupils);
        Mock<IMyPupilsWriteOnlyRepository> mockWriteRepository = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        IEnumerable<string> deletePupilUpnIdentifiers = upns.GetUniquePupilNumbers().Take(1).Select(t => t.Value);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: myPupilsId.Value,
            DeletePupilUpns: deletePupilUpnIdentifiers);

        // Act
        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, mockWriteRepository.Object);
        await useCase.HandleRequestAsync(request);

        // Assert
        readRepositoryMock.Verify(
            (repo) => repo.GetMyPupilsOrDefaultAsync(myPupilsId), Times.Once);

        IEnumerable<UniquePupilNumber> expectedListAfterDelete = upns.GetUniquePupilNumbers().Where(t => !deletePupilUpnIdentifiers.Contains(t.Value));

        mockWriteRepository.Verify(repo =>
            repo.SaveMyPupilsAsync(
                It.Is<MyPupilsAggregate>(
                    (myPupilsAggregate) => myPupilsAggregate.GetMyPupils().SequenceEqual(expectedListAfterDelete))), Times.Once);
    }

    [Fact]
    public async Task HandleRequestAsync_WhenNoUpnsMatch_ThrowsArgumentException()
    {
        // Arrange
        UniquePupilNumbers upns =
            UniquePupilNumbers.Create(uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 3));

        Mock<IMyPupilsReadOnlyRepository> readRepositoryMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsWriteOnlyRepository> mockWriteRepository = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        MyPupilsAggregate myPupils = MyPupilsAggregateTestDoubles.Create(upns);

        readRepositoryMock
            .Setup((repo) => repo.GetMyPupilsOrDefaultAsync(myPupils.AggregateId))
            .ReturnsAsync(myPupils);

        DeletePupilsFromMyPupilsUseCase useCase = new(readRepositoryMock.Object, mockWriteRepository.Object);

        DeletePupilsFromMyPupilsRequest request = new(
            UserId: myPupils.AggregateId.Value,
            DeletePupilUpns: UniquePupilNumberTestDoubles.Generate(count: 1).Select(t => t.Value));

        // Act 
        await Assert.ThrowsAsync<ArgumentException>(() => useCase.HandleRequestAsync(request));

        // Assert
        readRepositoryMock.Verify(
            (repo) => repo.GetMyPupilsOrDefaultAsync(
                myPupils.AggregateId), Times.Once);

        mockWriteRepository.Verify(repo =>
            repo.SaveMyPupilsAsync(myPupils), Times.Never);
    }
}
