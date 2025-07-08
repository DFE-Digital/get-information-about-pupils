using DfE.GIAP.Service.DsiApiClient;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.Security.Cryptography;
using System.Text;

namespace DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles
{
    public static class SecurityKeyProviderTestDoubles
    {
        public static Mock<ISecurityKeyProvider> SecurityKeyProviderMock() => new();

        public static ISecurityKeyProvider MockFor(
            string securityAlgorithm, SecurityKey securityKeyInstance)
        {
            var securityKeyProviderMock = SecurityKeyProviderMock();

            MockSecurityKeyTypeFor(securityKeyInstance, securityKeyProviderMock);
            MockSymmetricSecurityAlgorithmFor(securityAlgorithm, securityKeyProviderMock);

            return securityKeyProviderMock.Object;
        }

        public static void MockSecurityKeyTypeFor(
            SecurityKey securityKeyInstance,
            Mock<ISecurityKeyProvider> securityKeyProviderMock) =>
                securityKeyProviderMock
                    .Setup(securityKeyProvider =>
                        securityKeyProvider.SecurityKeyInstance)
                            .Returns(securityKeyInstance);

        public static void MockSymmetricSecurityAlgorithmFor(
            string securityAlgorithm,
            Mock<ISecurityKeyProvider> securityKeyProviderMock) =>
                securityKeyProviderMock
                    .Setup(securityKeyProvider =>
                        securityKeyProvider.SecurityAlgorithm)
                            .Returns(securityAlgorithm);

        public static SecurityKey SymmetricSecurityKeyInstanceFake =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes("fake_symmetric_secret_key_which_has_to_be_long"));

        public static string SymmetricSecurityAlgorithmFake => SecurityAlgorithms.HmacSha256Signature;

        public static SecurityKey AsymmetricSecurityKeyInstanceFake =>
            new RsaSecurityKey(RSA.Create());

        public static string AsymmetricSecurityAlgorithmFake => SecurityAlgorithms.RsaSha256;
    }
}
