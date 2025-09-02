using DfE.GIAP.Core.MyPupils.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.TestDoubles;
internal static class IMyPupilsReadOnlyRepositoryTestDoubles
{
    internal static Mock<IMyPupilsReadOnlyRepository> Default() => new();
}
