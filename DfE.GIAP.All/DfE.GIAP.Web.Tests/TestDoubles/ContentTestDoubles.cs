using DfE.GIAP.Core.Contents.Application.Models;

namespace DfE.GIAP.Web.Tests.TestDoubles;
// TODO generate with Faker
internal static class ContentTestDoubles
{
    internal static Content Default() => new()
    {
        Body = "Test body $%£%\"@{ \t \r \r\n }~",
        Title = "Test $%£^£ £\" \' \t \r \r\n title"
    };
}
