using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;

namespace DfE.GIAP.Service.Tests.DsiApiClient.TestDoubles
{
    public static class ConfigurationTestDoubles
    {
        public static Mock<IConfiguration> ConfigurationMock() => new();
        public static IOptions<T> OptionsMock<T>() where T : class, new()
        {
            return Options.Create(new T());
        }

        public static IConfiguration MockFor(string key, string value)
        {
            var configurationMock = ConfigurationMock();

            configurationMock
                .Setup(config =>
                    config[key]).Returns(value);

            return configurationMock.Object;
        }

        public static IOptions<T> MockForOptions<T>(T value) where T : class
        {
            var optionsMock = new Mock<IOptions<T>>();
            optionsMock.Setup(opt => opt.Value).Returns(value);
            return optionsMock.Object;
        }
    }
}
