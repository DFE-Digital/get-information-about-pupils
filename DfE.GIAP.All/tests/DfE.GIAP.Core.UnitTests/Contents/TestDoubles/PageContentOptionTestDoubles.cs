using DfE.GIAP.Core.Contents.Application.Options;

namespace DfE.GIAP.Core.UnitTests.Contents.TestDoubles;
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
