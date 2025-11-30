using System.Net;
using System.Net.Http.Json;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure;
using DfE.GIAP.Web.Features.Auth.Infrastructure.DataTransferObjects;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.Auth.Infrastructure;

public class DfeHttpSignInApiClientTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WithNullHttpClient()
    {
        Mock<IMapper<OrganisationDto, Organisation>> mockMapper = MapperTestDoubles.Default<OrganisationDto, Organisation>();
        Assert.Throws<ArgumentNullException>(() => new DfeHttpSignInApiClient(null!, mockMapper.Object));
    }

    [Fact]
    public async Task GetUserInfo_CallsCorrectUrl_And_DeserialisesResponse()
    {
        // Arrange
        Mock<IMapper<OrganisationDto, Organisation>> mockMapper = MapperTestDoubles.Default<OrganisationDto, Organisation>();
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
        DfeHttpSignInApiClient sut = new(client, mockMapper.Object);

        // Act
        UserAccess? result = await sut.GetUserInfo("svc1", "org1", "user1");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin", result!.Roles!.First().Code);
        Assert.Equal("/services/svc1/organisations/org1/users/user1", handler.CapturedRequests[0].RequestUri!.PathAndQuery);
    }

    [Fact]
    public async Task GetUserOrganisations_CallsCorrectUrl_And_MapperCalled()
    {
        // Arrange
        Mock<IMapper<OrganisationDto, Organisation>> mockMapper = MapperTestDoubles.Default<OrganisationDto, Organisation>();
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
        DfeHttpSignInApiClient sut = new(client, mockMapper.Object);

        // Act
        List<Organisation> result = await sut.GetUserOrganisations("user1");

        // Assert
        Assert.Single(result);
        Assert.Equal("/users/user1/organisations", handler.CapturedRequests[0].RequestUri!.PathAndQuery);
        mockMapper.Verify(
           (mapper) => mapper.Map(It.IsAny<OrganisationDto>()),
           Times.Exactly(expected.Count));
    }


    [Fact]
    public async Task GetUserOrganisation_ReturnsOrganisation_WhenFound()
    {
        // Arrange DTOs
        List<OrganisationDto> organisationDtos = new()
        {
            new OrganisationDto { Id = "org1", Name = "Org One" },
            new OrganisationDto { Id = "org2", Name = "Org Two" }
        };

        // Mock mapper to convert DTOs → Organisations based on Id/Name
        Mock<IMapper<OrganisationDto, Organisation>> mockMapper = new();
        mockMapper
            .Setup(m => m.Map(It.IsAny<OrganisationDto>()))
            .Returns<OrganisationDto>(dto => new Organisation { Id = dto.Id, Name = dto.Name });

        // Stub HTTP handler to return DTO JSON
        StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(organisationDtos)
            });

        HttpClient client = new(handler) { BaseAddress = new Uri("https://fake.local") };
        DfeHttpSignInApiClient sut = new(client, mockMapper.Object);

        // Act
        Organisation? result = await sut.GetUserOrganisation("user1", "org2");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Org Two", result!.Name);
    }

    [Fact]
    public async Task GetUserOrganisation_ReturnsNull_WhenNotFound()
    {
        // Arrange DTOs
        List<OrganisationDto> organisationDtos = new()
        {
            new OrganisationDto { Id = "org1", Name = "Org One" }
        };

        Mock<IMapper<OrganisationDto, Organisation>> mockMapper = new();
        mockMapper
            .Setup(m => m.Map(It.IsAny<OrganisationDto>()))
            .Returns<OrganisationDto>(dto => new Organisation { Id = dto.Id, Name = dto.Name });

        StubHttpMessageHandler handler = new(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(organisationDtos)
            });

        HttpClient client = new(handler) { BaseAddress = new Uri("https://fake.local") };
        DfeHttpSignInApiClient sut = new(client, mockMapper.Object);

        // Act
        Organisation? result = await sut.GetUserOrganisation("user1", "orgX"); // not present

        // Assert
        Assert.Null(result);
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
