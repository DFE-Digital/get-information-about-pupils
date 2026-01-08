using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.Features.MyPupils.Domain;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.DeleteAllMyPupils;
public sealed class DeleteAllMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<DeleteAllMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_WriteRepository_Is_Null()
    {
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        Func<DeleteAllMyPupilsUseCase> construct = () => new(readRepoMock.Object, null!);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        DeleteAllMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        //Act
        Func<Task> act = async () => await sut.HandleRequestAsync(request: null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task HandleAsync_NoWrites_When_MyPupilsAggregate_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(stub: null!);

        DeleteAllMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        // Act
        await sut.HandleRequestAsync(
            new DeleteAllMyPupilsRequest(UserId: "id"));

        readRepoMock.Verify(t => t.GetMyPupilsOrDefaultAsync(It.Is<MyPupilsId>(t => t.Value.Equals("id"))), Times.Once);
        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<MyPupilsAggregate>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_DeletesAllPupils()
    {
        MyPupilsId id = MyPupilsIdTestDoubles.Default();

        MyPupilsAggregate myPupilsAggregate =
            MyPupilsAggregateTestDoubles.Create(
                id,
                uniquePupilNumbers: UniquePupilNumbers.Create(UniquePupilNumberTestDoubles.Generate(count: 15)));


        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.MockForGetMyPupilsOrDefault(stub: myPupilsAggregate);

        DeleteAllMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        // Act
        await sut.HandleRequestAsync(
            new DeleteAllMyPupilsRequest(id.Value));

        readRepoMock.Verify(t => t.GetMyPupilsOrDefaultAsync(It.Is<MyPupilsId>(t => t.Value.Equals(id.Value))), Times.Once);
        writeRepoMock.Verify(t => t.SaveMyPupilsAsync(It.IsAny<MyPupilsAggregate>()), Times.Once);

        Assert.Equal(0, myPupilsAggregate.PupilCount);
        Assert.True(myPupilsAggregate.HasNoPupils);
        Assert.Empty(myPupilsAggregate.GetMyPupils());
    }
}
