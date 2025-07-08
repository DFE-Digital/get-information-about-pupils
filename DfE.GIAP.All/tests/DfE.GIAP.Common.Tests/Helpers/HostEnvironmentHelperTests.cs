using DfE.GIAP.Common.Helpers.HostEnvironment;
using Microsoft.Extensions.Hosting;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace DfE.GIAP.Common.Tests.Helpers
{
    public class HostEnvironmentHelperTests
    {
        [Fact]
        public void IsLocal_returns_true_when_environment_is_local()
        {
            // Arrange
            var hostEnv = Substitute.For<IHostEnvironment>();
            hostEnv.EnvironmentName.Returns("Local");

            // Act
            // Assert
            Assert.True(hostEnv.IsLocal());
        }


        [Fact]
        public void IsLocal_returns_false_when_environment_is_not_local()
        {
            // Arrange
            var hostEnv = Substitute.For<IHostEnvironment>();
            hostEnv.EnvironmentName.Returns("Test");

            // Act
            // Assert
            Assert.False(hostEnv.IsLocal());
        }

        [Theory]
        [InlineData("Development")]
        [InlineData("Test")]
        [InlineData("Local")]
        [InlineData("local")]
        public void ShouldShowErrors_returns_true_when_environment_is_not_production_tier(string envName)
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", envName);

            // Act
            // Assert
            Assert.True(HostEnvironmentHelper.ShouldShowErrors());
        }

        [Theory]
        [InlineData("Production")]
        public void ShouldShowErrors_returns_false_when_environment_is_production_tier(string envName)
        {
            // Arrange
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", envName);

            // Act
            // Assert
            Assert.False(HostEnvironmentHelper.ShouldShowErrors());
        }
    }
}
