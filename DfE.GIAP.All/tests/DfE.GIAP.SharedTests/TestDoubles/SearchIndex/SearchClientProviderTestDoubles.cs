using Azure.Search.Documents;
using DfE.GIAP.Core.MyPupils.Application.Search.Provider;
using Moq;

namespace DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
public static class SearchClientProviderTestDoubles
{
    public static Mock<ISearchClientProvider> Default() => new();

    public static Mock<ISearchClientProvider> MockFor<TResult>(Dictionary<string, List<TResult>> clientKeyToResults)
    {
        Mock<ISearchClientProvider> mock = Default();
        clientKeyToResults.ToList().ForEach(t =>
        {
            mock.Setup(
                (provider) => provider.InvokeSearchAsync<TResult>(
                    t.Key, It.IsAny<SearchOptions>()))
                        .ReturnsAsync(t.Value);
        });
        return mock;
    }
}
