using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Session;
using DfE.GIAP.Web.Session.Abstraction;
using DfE.GIAP.Web.Session.Abstraction.Command;
using DfE.GIAP.Web.Session.Abstraction.Query;
using DfE.GIAP.Web.Session.Infrastructure.AspNetCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace DfE.GIAP.Web.Tests.Session;
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
        Assert.NotNull(provider.GetService<ISessionObjectKeyResolver>());

        Assert.NotNull(scope.ServiceProvider.GetService<IAspNetCoreSessionProvider>());

        Assert.NotNull(scope.ServiceProvider.GetService<ISessionCommandHandler<SessionObjectReferenceType>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ISessionQueryHandler<SessionObjectReferenceType>>());
        Assert.NotNull(scope.ServiceProvider.GetService<ISessionObjectSerializer<SessionObjectReferenceType>>());
    }

    public record SessionObjectReferenceType { }
}
