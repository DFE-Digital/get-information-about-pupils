using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles;
using DfE.GIAP.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Home
{
    public class ServiceTimeoutControllerTests
    {
        [Fact]
        public void Constructor_throws_correct_exception_if_configuration_is_null()
        {
            Assert.Throws<ArgumentNullException>(() => new ServiceTimeoutController(null));
        }

        [Fact]
        public void KeepSessionAlive_returns_the_default_jsonResult()
        {
            // Arrange.
            var configurationMock = ConfigurationTestDoubles.OptionsMock<AzureAppSettings>();
            ServiceTimeoutController serviceTimeoutController = GetController(configurationMock);

            // Act
            var result = serviceTimeoutController.KeepSessionAlive();

            // Assert
            Assert.IsType<JsonResult>(result);
            JsonResult jsonResult = result;
            Assert.IsType<string>(jsonResult.Value);
            Assert.Equal("SessionPersisted", jsonResult.Value);
        }

        [Fact]
        public void SessionTimeoutValue_returns_the_configured_session_timeout_value()
        {
            // Arrange.
            const int appSettingValue = 5;
            IOptions<AzureAppSettings> configurationMock = ConfigurationTestDoubles.MockForOptions(new AzureAppSettings()
            {
                SessionTimeout = appSettingValue
            });

            ServiceTimeoutController serviceTimeoutController = GetController(configurationMock);

            // Act
            var result = serviceTimeoutController.SessionTimeoutValue();

            // Assert
            Assert.IsType<JsonResult>(result);
            JsonResult jsonResult = result;
            Assert.IsType<int>(jsonResult.Value);
            Assert.Equal(appSettingValue, jsonResult.Value);
        }

        [Fact]
        public void SessionTimeoutValue_returns_the_default_session_timeout_value_when_not_configured()
        {
            // Arrange.
            const int defaultSessionTimeoutValue = 20;
            IOptions<AzureAppSettings> configurationMock = ConfigurationTestDoubles.OptionsMock<AzureAppSettings>();

            ServiceTimeoutController serviceTimeoutController = GetController(configurationMock);

            // Act
            var result = serviceTimeoutController.SessionTimeoutValue();

            // Assert
            Assert.IsType<JsonResult>(result);
            JsonResult jsonResult = result;
            Assert.IsType<int>(jsonResult.Value);
            Assert.Equal(defaultSessionTimeoutValue, jsonResult.Value);
        }


        private ServiceTimeoutController GetController(IOptions<AzureAppSettings> configurationMock)
        {
            return new ServiceTimeoutController(configurationMock);
        }
    }
}
