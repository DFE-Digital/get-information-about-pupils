using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Web.Tests.Views;

public sealed class HostFixture : IAsyncLifetime
{
    private readonly GiapWebApplicationFactory _webApplicationFactory;

    public HostFixture()
    {
        _webApplicationFactory = new GiapWebApplicationFactory();
    }

    public IServiceProvider Services => _webApplicationFactory.Services;

    public HttpClient CreateClient() => _webApplicationFactory.CreateClient();

    public HttpClient CreateClient(WebApplicationFactoryClientOptions options) => _webApplicationFactory.CreateClient(options);

    public async Task<IHtmlDocument> RenderViewAsync<TModel>(string viewPath, Action<TModel> configureModel)
        where TModel : new()
    {
        using IServiceScope scope = Services.CreateScope();
        DefaultHttpContext httpContext = new() { RequestServices = scope.ServiceProvider };
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost");
        httpContext.SetEndpoint(new Endpoint(null, EndpointMetadataCollection.Empty, "View test"));

        ActionContext actionContext = new(httpContext, new RouteData(), new ActionDescriptor());
        IRazorViewEngine viewEngine = scope.ServiceProvider.GetRequiredService<IRazorViewEngine>();
        ViewEngineResult viewResult = viewEngine.GetView(null, viewPath, isMainPage: false);
        Assert.True(viewResult.Success, $"Could not find view {viewPath}");

        TModel model = new();
        configureModel(model);
        ViewDataDictionary<TModel> viewData = new(new EmptyModelMetadataProvider(), new ModelStateDictionary())
        {
            Model = model
        };
        using StringWriter writer = new();
        ViewContext viewContext = new(
            actionContext,
            viewResult.View,
            viewData,
            new TempDataDictionary(httpContext, scope.ServiceProvider.GetRequiredService<ITempDataProvider>()),
            writer,
            new HtmlHelperOptions());

        await viewResult.View.RenderAsync(viewContext);
        return new HtmlParser().ParseDocument(writer.ToString());
    }

    public Task InitializeAsync()
    {
        _ = Services;
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        _webApplicationFactory.Dispose();
        return Task.CompletedTask;
    }

    private sealed class GiapWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
                configuration.WithAzureSearchConnectionOptions());
        }
    }
}
