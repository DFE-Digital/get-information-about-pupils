using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.UnitTests.TestDoubles;

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
}
