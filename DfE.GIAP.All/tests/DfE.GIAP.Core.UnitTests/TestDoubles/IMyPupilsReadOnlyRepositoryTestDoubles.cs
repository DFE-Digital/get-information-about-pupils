using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class IMyPupilsReadOnlyRepositoryTestDoubles
{
    internal static Mock<IMyPupilsReadOnlyRepository> Default() => new();

    internal static Mock<IMyPupilsReadOnlyRepository> MockFor(Core.MyPupils.Application.Repositories.MyPupils stub)
    {
        Mock<IMyPupilsReadOnlyRepository> mock = Default();
        mock.Setup(
                (repo) => repo.GetMyPupilsAsync(
                    It.IsAny<UserId>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(stub);

        return mock;
    }
}
