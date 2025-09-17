using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.AddToMyPupils;
public sealed class AddPupilsToMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_WriteRepository_Is_Null()
    {
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        Func<AddPupilsToMyPupilsUseCase> construct = () => new(readRepoMock.Object, null!);

        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        Mock<IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();
        Mock<IMyPupilsReadOnlyRepository> readRepoMock = IMyPupilsReadOnlyRepositoryTestDoubles.Default();

        AddPupilsToMyPupilsUseCase sut = new(readRepoMock.Object, writeRepoMock.Object);

        //Act
        Func<Task> act = async () => await sut.HandleRequestAsync(request: null!);

        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }
}
