using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.Users.Application.Models;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class IMyPupilsReadOnlyRepositoryTestDoubles
{
    internal static Mock<IMyPupilsReadOnlyRepository> Default() => new();

    internal static Mock<IMyPupilsReadOnlyRepository> MockFor(Core.MyPupils.Application.Repositories.MyPupils stub)
    {
        Mock<IMyPupilsReadOnlyRepository> mock = Default();
        mock.Setup(
                (repo) => repo.GetMyPupilsOrDefaultAsync(
                    It.IsAny<UserId>(),
                    It.IsAny<CancellationToken>()))
            .ReturnsAsync(stub);

        return mock;
    }
}
