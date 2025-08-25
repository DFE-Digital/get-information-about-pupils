using System.Net;
using AngleSharp.Html.Dom;
using DfE.GIAP.Core.MyPupils.Application.Search.Options;
using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Dto;
using DfE.GIAP.Core.User.Infrastructure.Repository.Dtos;
using DfE.GIAP.SharedTests.Infrastructure.CosmosDb;
using DfE.GIAP.SharedTests.Infrastructure.SearchIndex;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.IntegrationTests.Helpers;
using DfE.GIAP.Web.IntegrationTests.WebApplicationFactory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Web.IntegrationTests;

[Collection(WebIntegrationTestsCollectionMarker.Name)]
public sealed class MyPupilsIntegrationTests
{
    private readonly CosmosDbFixture _fixture;

    public MyPupilsIntegrationTests(CosmosDbFixture fixture)
    {
        _fixture = fixture;
    }

    //[Fact] TODO fix SearchOptions to use dynamic port
    public async Task GET_Index_Returns_OK()
    {
        using GetInformationAboutPupilsWebApplicationFactory web = new();

        using SearchIndexFixture fixture = new(
            web.Services.GetRequiredService<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdPupils = fixture.StubNpdSearchIndex();
        fixture.StubPupilPremiumSearchIndex();

        HttpClient httpClient = web.CreateClient();
        
        UserDto user = UserDtoTestDoubles.WithPupils("MY_DSI_ID", [new(npdPupils.First().UPN)]);

        await _fixture.Database.WriteItemAsync(user);

        HttpResponseMessage response = await httpClient.GetAsync("/my-pupil-list");
        IHtmlDocument document = await HtmlHelpers.GetDocumentAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("My pupil list", document.QuerySelector("#mypupils-heading").TextContent.ReplaceLineEndings().Trim());

        Assert.NotNull(document.QuerySelector("#pupil--list--table"));
    }
}
