using DfE.GIAP.Core.Contents.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.Contents.TestDoubles;
internal sealed class ContentReadOnlyRepositoryTestDoubles
{
    internal static Mock<IContentReadOnlyRepository> Default() => new();
}
