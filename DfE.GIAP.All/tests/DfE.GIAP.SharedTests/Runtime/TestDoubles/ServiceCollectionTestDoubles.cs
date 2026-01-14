using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.SharedTests.Runtime.TestDoubles;
public static class ServiceCollectionTestDoubles
{
    public static IServiceCollection Default() => new ServiceCollection();
}
