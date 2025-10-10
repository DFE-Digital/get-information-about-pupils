using DfE.GIAP.Core.MyPupils.Application.Repositories;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class IMyPupilsReadOnlyRepositoryTestDoubles
{
    internal static Mock<IMyPupilsReadOnlyRepository> Default() => new();

    internal static Mock<IMyPupilsReadOnlyRepository> MockForGetMyPupils(Core.MyPupils.Domain.AggregateRoot.MyPupils stub)
    {
        Mock<IMyPupilsReadOnlyRepository> mock = Default();
        mock.Setup(
                (repo) => repo.GetMyPupils(It.IsAny<MyPupilsId>()))
            .ReturnsAsync(stub);

        return mock;
    }

    internal static Mock<IMyPupilsReadOnlyRepository> MockForGetMyPupilsOrDefault(Core.MyPupils.Domain.AggregateRoot.MyPupils? stub)
    {
        Mock<IMyPupilsReadOnlyRepository> mock = Default();
        mock.Setup(
                (repo) => repo.GetMyPupilsOrDefaultAsync(It.IsAny<MyPupilsId>()))
            .ReturnsAsync(stub);

        return mock;
    }
}
