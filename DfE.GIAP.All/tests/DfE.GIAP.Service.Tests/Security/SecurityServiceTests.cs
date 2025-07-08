using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.SecurityReports;
using DfE.GIAP.Service.ApiProcessor;
using DfE.GIAP.Service.Security;
using DfE.GIAP.Service.Tests.FakeData;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Service.Tests.Security
{
    [Trait("Category", "Security Service Unit Tests")]
    public class SecurityServiceTests : IClassFixture<SecurityServiceResultsFake>
    {
        private readonly SecurityServiceResultsFake _securityServiceResultsFake;

        public SecurityServiceTests(SecurityServiceResultsFake securityServiceResultsFake)
        {
            _securityServiceResultsFake = securityServiceResultsFake;
        }

        [Fact]
        public async Task GetAllLocalAuthoritiesReturnsListOfLocalAuthorities()
        {
            // Arrange
            var settings = _securityServiceResultsFake.GetAppSettings();

            var mockApiProcessorService = new Mock<IApiService>();
            mockApiProcessorService.Setup(x => x.GetToListAsync<LocalAuthority>(It.IsAny<Uri>())).ReturnsAsync(_securityServiceResultsFake.GetListOfAllLocalAuthorities());

            var securityService = new SecurityService(mockApiProcessorService.Object, Options.Create(settings));

            var expected = _securityServiceResultsFake.GetListOfAllLocalAuthorities();

            // Act
            var actual = await securityService.GetAllLocalAuthorities();

            // Assert
            Assert.IsAssignableFrom<List<LocalAuthority>>(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected[0].Name, actual[0].Name);
            Assert.Equal(expected[0].Code, actual[0].Code);
            Assert.Equal(expected[0].Description, actual[0].Description);
            mockApiProcessorService.Verify(x => x.GetToListAsync<LocalAuthority>(It.IsAny<Uri>()), Times.Once());
        }

        [Fact]
        public async Task GetAllAcademyTrustsReturnsListOfAcademyTrusts()
        {
            // Arrange
            var settings = _securityServiceResultsFake.GetAppSettings();

            var mockApiProcessorService = new Mock<IApiService>();
            mockApiProcessorService.Setup(x => x.PostAsync<AcademyRequest, List<AcademyTrust>>(It.IsAny<Uri>(), It.IsAny<AcademyRequest>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(_securityServiceResultsFake.GetListOfAllAcademyTrusts());

            var securityService = new SecurityService(mockApiProcessorService.Object, Options.Create(settings));

            var expected = _securityServiceResultsFake.GetListOfAllAcademyTrusts();

            // Act
            var actual = await securityService.GetAcademyTrusts(new List<string>() { "MAT", "SAT" });

            // Assert
            Assert.IsAssignableFrom<List<AcademyTrust>>(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected[0].Name, actual[0].Name);
            Assert.Equal(expected[0].Code, actual[0].Code);
            Assert.Equal(expected[0].Description, actual[0].Description);
            mockApiProcessorService.Verify(x => x.PostAsync<AcademyRequest, List<AcademyTrust>>(It.IsAny<Uri>(), It.IsAny<AcademyRequest>(), It.IsAny<AzureFunctionHeaderDetails>()), Times.Once());
        }

        [Fact]
        public async Task GetEstablishmentsByLocalAuthorityCodeReturnsListOfEstablishments()
        {
            // Arrange
            var settings = _securityServiceResultsFake.GetAppSettings();
            var localAuthorityCode = "001";
            //var expectedRequest = new EstablishmentRequest { Code = Int32.Parse(localAuthorityCode), Type = 2 };
            var expectedRequest = new EstablishmentRequest { Code = localAuthorityCode, Type = "LA" };

            var mockApiProcessorService = new Mock<IApiService>();
            mockApiProcessorService.Setup(x =>
            x.PostAsync<EstablishmentRequest, EstablishmentResponse>(It.IsAny<Uri>(), It.Is<EstablishmentRequest>(x => x.Code == expectedRequest.Code && x.Type == expectedRequest.Type), It.IsAny<AzureFunctionHeaderDetails>()))
                .ReturnsAsync(_securityServiceResultsFake.GetEstablishmentResponse());


            var securityService = new SecurityService(mockApiProcessorService.Object, Options.Create(settings));

            var expected = _securityServiceResultsFake.GetEstablishmentResponse().Establishments.ToList();

            // Act
            var actual = await securityService.GetEstablishmentsByOrganisationCode("LA", localAuthorityCode);

            // Assert
            Assert.IsAssignableFrom<List<Establishment>>(actual);
            Assert.Equal(expected.Count, actual.Count);
            Assert.Equal(expected[0].Name, actual[0].Name);
            Assert.Equal(expected[0].URN, actual[0].URN);
            Assert.Equal(expected[0].Description, actual[0].Description);
            mockApiProcessorService.Verify(x => x.PostAsync<EstablishmentRequest, EstablishmentResponse>(It.IsAny<Uri>(), It.IsAny<EstablishmentRequest>(), It.IsAny<AzureFunctionHeaderDetails>()), Times.Once());
        }

        [Fact]
        public async Task GetEstablishmentsByAcademyTrustCode()
        {
            // Arrange
            var settings = _securityServiceResultsFake.GetAppSettings();
            var academyTrustCode = "001";
            var docTypes = new List<string>() { "MAT", "SAT" };
            var expectedRequest = new AcademyRequest { DocTypes = docTypes, Id = academyTrustCode, IncludeEstablishments = true };

            var mockApiProcessorService = new Mock<IApiService>();
            mockApiProcessorService.Setup(x => x.PostAsync<AcademyRequest, List<AcademyTrust>>(It.IsAny<Uri>(), It.IsAny<AcademyRequest>(), It.IsAny<AzureFunctionHeaderDetails>()))
                .ReturnsAsync(_securityServiceResultsFake.GetEstablishmentsByAcademyTrustCode(docTypes, academyTrustCode));

            var securityService = new SecurityService(mockApiProcessorService.Object, Options.Create(settings));
            
            var expected = _securityServiceResultsFake.GetEstablishmentsByAcademyTrustCode(docTypes, academyTrustCode).ToList();
            var expectedAcademyTrustEstablishment = expected[0].Establishments;

            // Act
            var actual = await securityService.GetEstablishmentsByAcademyTrustCode(docTypes, academyTrustCode);

            // Assert
            Assert.IsAssignableFrom<List<Establishment>>(actual);
            Assert.Equal(expectedAcademyTrustEstablishment.Count(), actual.Count());
            Assert.Equal(expectedAcademyTrustEstablishment.ElementAt(0).Name, actual[0].Name);
            Assert.Equal(expectedAcademyTrustEstablishment.ElementAt(0).URN, actual[0].URN);
            Assert.Equal(expectedAcademyTrustEstablishment.ElementAt(0).Description, actual[0].Description);
            mockApiProcessorService.Verify(x => x.PostAsync<AcademyRequest, List<AcademyTrust>>(It.IsAny<Uri>(), It.IsAny<AcademyRequest>(), It.IsAny<AzureFunctionHeaderDetails>()), Times.Once());
        }
    }
}
