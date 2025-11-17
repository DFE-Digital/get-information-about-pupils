using System.Net;
using System.Net.Http.Json;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure;

public class DfeHttpSignInApiClientTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullHttpClient()
    {
        Assert.Throws<ArgumentNullException>(() => new DfeHttpSignInApiClient(null!));
    }

    [Fact]
    public async Task GetUserInfo_CallsCorrectUrl_And_DeserialisesResponse()
    {
        // Arrange
        UserAccess expected = new()
        {
            Roles = new List<UserRole> {
                new UserRole { Code = "Admin" }
            }
        };

        StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) });

        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://fake.local")
        };
        DfeHttpSignInApiClient sut = new(client);

        // Act
        UserAccess? result = await sut.GetUserInfo("svc1", "org1", "user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result!.Roles!.First().Code);
        Assert.Equal("/services/svc1/organisations/org1/users/user1", handler.CapturedRequests[0].RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task GetUserOrganisations_CallsCorrectUrl_And_DeserialisesResponse()
    {
        // Arrange
        List<Organisation> expected = new() {
            new Organisation {
                Id = "org1",
                Name = "Test Org"
            }
        };
        StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(expected) });

        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://fake.local")
        };
        DfeHttpSignInApiClient sut = new(client);

        // Act
        List<Organisation> result = await sut.GetUserOrganisations("user1");

        // Assert
        Assert.Single(result);
        Assert.Equal("org1", result[0].Id);
        Assert.Equal("/users/user1/organisations", handler.CapturedRequests[0].RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task GetUserOrganisation_FiltersOrganisationCorrectly()
    {
        // Arrange
        List<Organisation> organisations = new()
        {
            new Organisation { Id = "org1", Name = "Org One" },
            new Organisation { Id = "org2", Name = "Org Two" }
        };

        StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(organisations) });

        HttpClient client = new(handler)
        {
            BaseAddress = new Uri("https://fake.local")
        };
        DfeHttpSignInApiClient sut = new(client);

        // Act
        Organisation? result = await sut.GetUserOrganisation("user1", "org2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Org Two", result!.Name);
        Assert.Equal("/users/user1/organisations", handler.CapturedRequests[0].RequestUri!.PathAndQuery);
    }

    private class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public List<HttpRequestMessage> CapturedRequests { get; } = new();

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            CapturedRequests.Add(request);
            return Task.FromResult(_handler(request));
        }
    }
}
