using DfE.GIAP.Core.Content.Application.Options;

namespace DfE.GIAP.Core.UnitTests.Content.TestDoubles;
internal static class PageContentOptionTestDoubles
{
    public static PageContentOption Default() => new();
    public static PageContentOption StubFor(string documentId)
    {
        PageContentOption stub = Default();
        stub.DocumentId = documentId;
        return stub;
    }
}
