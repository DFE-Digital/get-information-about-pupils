using DfE.GIAP.Core.MyPupils.Application.Search.Options;

namespace DfE.GIAP.Core.SharedTests;

public static class SearchIndexOptionsFactory
{
#pragma warning disable S1075 // URIs should not be hardcoded
    public static SearchIndexOptions SearchOptionsStub() => new()
    {
        Url = "idx-further-education-v3",
        Key = "",
        Indexes = []
};
#pragma warning restore S1075 // URIs should not be hardcoded
}
