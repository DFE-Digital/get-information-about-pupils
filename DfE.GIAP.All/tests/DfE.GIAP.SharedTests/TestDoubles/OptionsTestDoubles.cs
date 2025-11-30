using Microsoft.Extensions.Options;
using Moq;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class OptionsTestDoubles
{
    public static IOptions<T> Default<T>() where T : class, new()
    {
        return MockAs(new T());
    }

    public static IOptions<T> MockNullOptions<T>() where T : class
    {
        return MockAs<T>(value: null);
    }

    public static IOptions<T> MockAs<T>(T? value) where T : class
    {
        Mock<IOptions<T>> mock = new();

        mock.Setup(t => t.Value)
            .Returns(value!)
            .Verifiable();

        return mock.Object;
    }

    public static IOptions<T> MockAs<T>(Action<T> configure) where T : class, new()
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
}
