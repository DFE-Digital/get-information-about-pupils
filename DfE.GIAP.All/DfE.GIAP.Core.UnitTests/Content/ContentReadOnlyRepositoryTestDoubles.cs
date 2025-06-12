using DfE.GIAP.Core.Content.Application.Repository;

namespace DfE.GIAP.Core.UnitTests.Content;
internal sealed class ContentReadOnlyRepositoryTestDoubles
{
    internal static Mock<IContentReadOnlyRepository> Default() => new();
}
