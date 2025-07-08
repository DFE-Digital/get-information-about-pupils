using DfE.GIAP.Domain.Models.User;
using DfE.GIAP.Service.DsiApiClient;
using DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles;
using DfE.GIAP.Service.Tests.FakeData;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Service.Tests.DsiApiClient
{
    public class DfeSignInApiClientTests
    {
        [Fact]
        public void DfeSignInApiClient_throws_null_on_client_null()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DfeSignInApiClient(dsiHttpClientProvider: null));
        }

        [Fact]
        public async Task GetUserInfo_with_valid_request_uri_returns_configured_user_access_instance()
        {
            // Arrange
           var testUserResponseObject = DSIClientDataFakes.GetValidSingleDataAsString();

            IDsiHttpClientProvider testDsiHttpClientProvider =
                DsiHttpClientProviderTestDouble.MockFor(
                    DsiHttpClientProviderTestDouble.MockDsiHttpClient(
                        testUserResponseObject, HttpStatusCode.OK));

            IDfeSignInApiClient dfeSignInApiClient =
                new DfeSignInApiClient(testDsiHttpClientProvider);

            // Act
            var userAccessObject =
                await dfeSignInApiClient.GetUserInfo(
                    "test_service_id", "test_organisation_id", "test_user_id");

            // Assert
            Assert.NotNull(userAccessObject);
            Assert.IsType<UserAccess>(userAccessObject);
            Assert.Equal(DSIClientDataFakes.GetValidSingleData().UserId, userAccessObject.UserId);
            Assert.Single(userAccessObject.Roles);
        }

        [Fact]
        public async Task GetUserInfo_with_invalid_request_uri_returns_null()
        {
            // Arrange
            var testUserResponseObject = DSIClientDataFakes.GetValidSingleDataAsString();

            IDsiHttpClientProvider testDsiHttpClientProvider =
                DsiHttpClientProviderTestDouble.MockFor(
                    DsiHttpClientProviderTestDouble.MockDsiHttpClient(
                        testUserResponseObject, HttpStatusCode.NotFound));

            IDfeSignInApiClient dfeSignInApiClient =
                new DfeSignInApiClient(testDsiHttpClientProvider);

            // Act
            var userAccessObject =
                await dfeSignInApiClient.GetUserInfo(
                    "test_service_id", "test_organisation_id", "test_user_id");

            // Assert
            Assert.Null(userAccessObject);
        }

        [Fact]
        public async Task GetUserOrganisation_with_valid_request_returns_configured_organisation_instance()
        {
            // Arrange
            var testOrganisationResponseObject = DSIClientDataFakes.GetOrganisationsAsString();

            IDsiHttpClientProvider testDsiHttpClientProvider =
                DsiHttpClientProviderTestDouble.MockFor(
                    DsiHttpClientProviderTestDouble.MockDsiHttpClient(
                        testOrganisationResponseObject, HttpStatusCode.OK));

            IDfeSignInApiClient dfeSignInApiClient =
                new DfeSignInApiClient(testDsiHttpClientProvider);

            // Act
            var organisationObject =
                await dfeSignInApiClient.GetUserOrganisation("", DSIClientDataFakes.GetOrganisations().First().Id);

            // Assert
            Assert.NotNull(organisationObject);
            Assert.IsType<Organisation>(organisationObject);
            Assert.Equal(DSIClientDataFakes.GetOrganisations().First().Id, organisationObject.Id);
            Assert.Equal(DSIClientDataFakes.GetOrganisations().First().Name, organisationObject.Name);
        }

        [Fact]
        public async Task GetUserOrganisation_with_invalid_request_returns_null()
        {
            // Arrange
            var testOrganisationResponseObject = DSIClientDataFakes.GetOrganisationsAsString();

            IDsiHttpClientProvider testDsiHttpClientProvider =
                DsiHttpClientProviderTestDouble.MockFor(
                    DsiHttpClientProviderTestDouble.MockDsiHttpClient(
                        testOrganisationResponseObject, HttpStatusCode.NotFound));

            IDfeSignInApiClient dfeSignInApiClient =
                new DfeSignInApiClient(testDsiHttpClientProvider);

            // Act
            var organisationObject =
                await dfeSignInApiClient.GetUserOrganisation("", DSIClientDataFakes.GetOrganisations().First().Id);

            // Assert
            Assert.Null(organisationObject);
        }
    }
}
