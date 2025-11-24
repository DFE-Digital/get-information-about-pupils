using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.Auth.Application;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure.DataTransferObjects;
using DfE.GIAP.Web.Features.Auth.Infrastructure.Mappers;
using Newtonsoft.Json;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure;

public class DfeHttpSignInApiClient : IDfeSignInApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IMapper<OrganisationDto, Organisation> _organisationMapper;

    public DfeHttpSignInApiClient(
        HttpClient httpClient,
        IMapper<OrganisationDto, Organisation> organisationMapper)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(organisationMapper);
        _httpClient = httpClient;
        _organisationMapper = organisationMapper;
    }

    public async Task<UserAccess?> GetUserInfo(string serviceId, string organisationId, string userId)
    {
        string url = $"/services/{serviceId}/organisations/{organisationId}/users/{userId}";
        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserAccess>();
    }

    public async Task<Organisation?> GetUserOrganisation(string userId, string organisationId)
    {
        // Fetch the list of organisations
        List<Organisation> organisations = await GetUserOrganisations(userId);
        return organisations?.Find(o => o.Id == organisationId);
    }

    public async Task<List<Organisation>> GetUserOrganisations(string userId)
    {
        string url = $"/users/{userId}/organisations";

        HttpResponseMessage response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        string json = await response.Content.ReadAsStringAsync();
        List<OrganisationDto> organisationDtos = JsonConvert
            .DeserializeObject<List<OrganisationDto>>(json) ?? new List<OrganisationDto>();

        return organisationDtos.Select(_organisationMapper.Map).ToList();
    }
}
