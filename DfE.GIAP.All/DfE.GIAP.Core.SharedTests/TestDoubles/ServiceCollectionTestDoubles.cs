using DfE.GIAP.Core.NewsArticles;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.SharedTests.TestDoubles;
public static class ServiceCollectionTestDoubles
{
    public static IServiceCollection Default() => new ServiceCollection();
}
