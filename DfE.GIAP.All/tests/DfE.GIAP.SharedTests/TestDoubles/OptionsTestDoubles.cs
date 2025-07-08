using Microsoft.Extensions.Options;
using Moq;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class OptionsTestDoubles
{
    public static IOptions<T> Default<T>() where T : class, new()
    {
        return ConfigureOptions(new T());
    }

    public static IOptions<T> ConfigureOptionsWithNullValue<T>() where T : class
    {
        return ConfigureOptions<T>(value: null);
    }

    public static IOptions<T> ConfigureOptions<T>(Action<T> configure) where T : class, new()
    {
        Mock<IOptions<T>> mock = new();

        mock.Setup((t) => t.Value)
            .Returns(() =>
            {
                T configurable = new();
                configure(configurable);
                return configurable;
            })
            .Verifiable();

        return mock.Object;
    }

    private static IOptions<T> ConfigureOptions<T>(T? value) where T : class
    {
        Mock<IOptions<T>> mock = new();
        mock.Setup(t => t.Value).Returns(value!).Verifiable();
        return mock.Object;
    }
}
