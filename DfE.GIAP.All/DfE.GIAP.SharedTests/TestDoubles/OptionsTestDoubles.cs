using Microsoft.Extensions.Options;
using Moq;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class OptionsTestDoubles
{
    public static IOptions<T> Default<T>() where T : class, new()
    {
        return WithValue(new T());
    }

    public static IOptions<T> WithNullValue<T>() where T : class
    {
        return WithValue<T>(null);
    }

    public static IOptions<T> WithValue<T>(T? value) where T : class
    {
        Mock<IOptions<T>> mock = new();
        mock.Setup(t => t.Value).Returns(value!).Verifiable();
        return mock.Object;
    }
}
