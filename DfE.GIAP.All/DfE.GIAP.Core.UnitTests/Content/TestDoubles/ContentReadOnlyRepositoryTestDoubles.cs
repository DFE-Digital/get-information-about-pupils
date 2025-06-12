using DfE.GIAP.Core.Content.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.Content.TestDoubles;
internal sealed class ContentReadOnlyRepositoryTestDoubles
{
    internal static Mock<IContentReadOnlyRepository> Default() => new();
}
