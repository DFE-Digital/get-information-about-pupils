using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.UnitTests.Search.Infrastructure.TestDoubles;

/// <summary>
/// Provides helper methods for creating mocked <see cref="IOptions{TOptions}"/> instances
/// for use in unit tests. This allows tests to inject configuration objects without
/// relying on actual configuration files or the options binding pipeline.
/// </summary>
internal static class IOptionsTestDouble
{
    /// <summary>
    /// Creates a new <see cref="Mock{IOptions}"/> for the specified options type.
    /// </summary>
    /// <typeparam name="TOptionSetting">
    /// The type of the options settings class to mock.
    /// Must be a reference type.
    /// </typeparam>
    /// <returns>
    /// A new <see cref="Mock{IOptions}"/> instance for the given options type.
    /// </returns>
    public static Mock<IOptions<TOptionSetting>> IOptionsMock<TOptionSetting>()
        where TOptionSetting : class => new();

    /// <summary>
    /// Creates a mocked <see cref="IOptions{TOptions}"/> instance that will return
    /// the specified <paramref name="optionsSettings"/> when its <see cref="IOptions{TOptions}.Value"/> property is accessed.
    /// </summary>
    /// <typeparam name="TOptionsSetting">
    /// The type of the options settings class to mock.
    /// Must be a reference type.
    /// </typeparam>
    /// <param name="optionsSettings">
    /// The options settings instance to be returned by the mock.
    /// </param>
    /// <returns>
    /// A configured <see cref="IOptions{TOptions}"/> mock object that returns the provided settings.
    /// </returns>
    public static IOptions<TOptionsSetting> IOptionsMockFor<TOptionsSetting>(TOptionsSetting optionsSettings)
        where TOptionsSetting : class
    {
        // Create a mock for IOptions<TOptionsSetting>
        Mock<IOptions<TOptionsSetting>> optionsMock = IOptionsMock<TOptionsSetting>();

        // Configure the mock to return the provided settings instance when Value is accessed
        optionsMock
            .Setup(options => options.Value)
            .Returns(optionsSettings);

        // Return the configured mock object
        return optionsMock.Object;
    }
}
