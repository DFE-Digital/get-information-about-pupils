using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.Users.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

namespace DfE.GIAP.Web.IntegrationTests;

[Collection(WebIntegrationTestsCollectionMarker.Name)]
public sealed class MyPupilsIntegrationTests
{
    private readonly CosmosDbFixture _fixture;

    public MyPupilsIntegrationTests(CosmosDbFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact] //TODO fix SearchOptions to use dynamic port
    public async Task GET_Index_Returns_OK()
    {
        await using GetInformationAboutPupilsWebApplicationFactory webApplicationFactory = new();

        using SearchIndexFixture searchIndex = new(
            webApplicationFactory.Services.GetRequiredService<IOptions<SearchIndexOptions>>());

        IEnumerable<AzureIndexEntity> npdPupils = await searchIndex.StubNpdSearchIndex();
        _ = await searchIndex.StubPupilPremiumSearchIndex();

        HttpClient httpClient = webApplicationFactory.CreateClient();

        MyPupilsDocumentDto seededMyPupilsDocument = MyPupilsDocumentDtoTestDoubles.Create(
            id: MyPupilsIdTestDoubles.Create("MY_DSI_ID"),
            upns:
                UniquePupilNumbers.Create(
                    npdPupils.Select((indexUpn) => new UniquePupilNumber(indexUpn.id))));

        await _fixture.Database.WriteItemAsync(seededMyPupilsDocument);

        HttpResponseMessage response = await httpClient.GetAsync("/my-pupil-list");
        IHtmlDocument document = await HtmlHelpers.GetDocumentAsync(response);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        string actualHeading = document.QuerySelector("#mypupils-heading")!.TextContent.ReplaceLineEndings().Trim();
        Assert.Equal("My pupil list", actualHeading);

        Assert.NotNull(document.QuerySelector("#pupil--list--table"));
    }
}
