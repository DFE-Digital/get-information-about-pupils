using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Shared.Session;
using DfE.GIAP.Web.Shared.Session.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Web.Tests.Shared.Session;
public sealed class CompositionRootTests
{
    [Fact]
    public void AddAspNetCoreSession_Resolves_Services()
    {
        IServiceProvider provider =
            ServiceCollectionTestDoubles.Default()
                .AddSingleton<IHttpContextAccessor>((sp) => new HttpContextAccessor())
                .AddAspNetCoreSessionServices()
                .BuildServiceProvider();

        using IServiceScope scope = provider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetService<IAspNetCoreSessionProvider>());
    }
}
