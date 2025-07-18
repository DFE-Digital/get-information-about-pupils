using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Http;
using Moq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DfE.GIAP.Web.Tests.Providers;

public class SessionProviderTests
{
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ISession> _sessionMock;
    private readonly DefaultHttpContext _httpContext;
    private readonly SessionProvider _sessionProvider;

    public SessionProviderTests()
    {
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _sessionMock = new Mock<ISession>();
        _httpContext = new DefaultHttpContext { Session = _sessionMock.Object };
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContext);

        _sessionProvider = new SessionProvider(_httpContextAccessorMock.Object);
    }

    [Fact]
    public void SetSessionValue_SetsCorrectValue()
    {
        string key = "TestKey";
        string value = "TestValue";
        byte[] expectedBytes = Encoding.UTF8.GetBytes(value);

        _sessionMock.Setup(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))));

        _sessionProvider.SetSessionValue(key, value);

        _sessionMock.Verify(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))), Times.Once);
    }

    [Fact]
    public void GetSessionValue_ReturnsCorrectValue()
    {
        string key = "TestKey";
        string value = "TestValue";
        byte[]? bytes = Encoding.UTF8.GetBytes(value);

        _sessionMock.Setup(s => s.TryGetValue(key, out bytes)).Returns(true);

        string result = _sessionProvider.GetSessionValue(key);

        Assert.Equal(value, result);
    }

    [Fact]
    public void RemoveSessionValue_RemovesCorrectKey()
    {
        string key = "TestKey";

        _sessionProvider.RemoveSessionValue(key);

        _sessionMock.Verify(s => s.Remove(key), Times.Once);
    }

    [Fact]
    public void ContainsSessionKey_ReturnsTrue_IfKeyExists()
    {
        string key = "TestKey";
        _sessionMock.Setup(s => s.Keys).Returns(new List<string> { "TestKey" });

        bool result = _sessionProvider.ContainsSessionKey(key);

        Assert.True(result);
    }

    [Fact]
    public void ContainsSessionKey_ReturnsFalse_IfKeyDoesNotExist()
    {
        string key = "MissingKey";
        _sessionMock.Setup(s => s.Keys).Returns(new List<string> { "TestKey" });

        bool result = _sessionProvider.ContainsSessionKey(key);

        Assert.False(result);
    }

    [Fact]
    public void ClearSession_RemovesAllKeys()
    {
        List<string> keys = new List<string> { "Key1", "Key2" };
        _sessionMock.Setup(s => s.Keys).Returns(keys);

        _sessionProvider.ClearSession();

        foreach (string key in keys)
        {
            _sessionMock.Verify(s => s.Remove(key), Times.Once);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Methods_ThrowArgumentNullException_WhenKeyIsNullOrEmpty(string invalidKey)
    {
        Assert.Throws<ArgumentNullException>(() => _sessionProvider.SetSessionValue(invalidKey, "val"));
        Assert.Throws<ArgumentNullException>(() => _sessionProvider.GetSessionValue(invalidKey));
        Assert.Throws<ArgumentNullException>(() => _sessionProvider.RemoveSessionValue(invalidKey));
        Assert.Throws<ArgumentNullException>(() => _sessionProvider.ContainsSessionKey(invalidKey));
    }

    [Fact]
    public void Throws_When_HttpContext_Is_Null()
    {
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns((HttpContext)null);
        SessionProvider provider = new SessionProvider(_httpContextAccessorMock.Object);

        Assert.Throws<InvalidOperationException>(() => provider.GetSessionValue("key"));
    }

    [Fact]
    public void Throws_When_Session_Is_Null()
    {
        _httpContext.Session = null;
        var provider = new SessionProvider(_httpContextAccessorMock.Object);

        Assert.Throws<InvalidOperationException>(() => provider.SetSessionValue("key", "val"));
    }

    [Fact]
    public void SetSessionObject_SerializesAndStoresJson_UsingSet()
    {
        string key = "obj";
        TestObject obj = new TestObject { Name = "John", Age = 30 };
        string json = JsonSerializer.Serialize(obj);
        byte[] expectedBytes = Encoding.UTF8.GetBytes(json);

        _sessionMock.Setup(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))));

        _sessionProvider.SetSessionObject(key, obj);

        _sessionMock.Verify(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))), Times.Once);
    }

    [Fact]
    public void GetSessionObject_DeserializesStoredJson()
    {
        string key = "obj";
        TestObject obj = new TestObject { Name = "Jane", Age = 25 };
        string json = JsonSerializer.Serialize(obj);
        byte[]? bytes = Encoding.UTF8.GetBytes(json);

        _sessionMock.Setup(s => s.TryGetValue(key, out bytes)).Returns(true);

        TestObject result = _sessionProvider.GetSessionObject<TestObject>(key);

        Assert.Equal(obj, result);
    }

    [Fact]
    public void GetSessionObject_ReturnsDefault_WhenKeyNotFound()
    {
        string key = "missing";
        byte[] outBytes = null;

        _sessionMock.Setup(s => s.TryGetValue(key, out outBytes)).Returns(false);

        TestObject result = _sessionProvider.GetSessionObject<TestObject>(key);

        Assert.Null(result);
    }


    private class TestObject
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public override bool Equals(object obj)
        {
            return obj is TestObject other && Name == other.Name && Age == other.Age;
        }
    }
}
