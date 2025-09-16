using DfE.GIAP.Core.MyPupils.Application.UseCases.DeleteAllPupilsFromMyPupils;
using DfE.GIAP.Core.UnitTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.DeleteAllMyPupils;
public sealed class DeleteAllMyPupilsUseCaseTests
{
    [Fact]
    public void Constructor_Throws_When_ReadRepository_Is_Null()
    {
        Mock<Core.MyPupils.Application.Repositories.IMyPupilsWriteOnlyRepository> writeRepoMock = IMyPupilsWriteOnlyRepositoryTestDoubles.Default();

        Func<DeleteAllMyPupilsUseCase> construct = () => new(null!, writeRepoMock.Object);

        Assert.Throws<ArgumentNullException>(construct);
    }

    //[Fact]
    //public void Constructor_Throws_When_WriteR  epository_Is_Null()
    //{
    //    DeleteAllMyPupilsUseCase

    //}
}
