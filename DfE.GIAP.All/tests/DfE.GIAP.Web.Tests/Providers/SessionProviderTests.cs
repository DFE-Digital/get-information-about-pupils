using DfE.GIAP.Web.Providers.Session;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using Xunit;

namespace DfE.GIAP.Web.Tests.Providers
{
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
            var key = "TestKey";
            var value = "TestValue";
            var expectedBytes = Encoding.UTF8.GetBytes(value);

            _sessionMock.Setup(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))));

            _sessionProvider.SetSessionValue(key, value);

            _sessionMock.Verify(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))), Times.Once);
        }

        [Fact]
        public void GetSessionValue_ReturnsCorrectValue()
        {
            var key = "TestKey";
            var value = "TestValue";
            var bytes = Encoding.UTF8.GetBytes(value);

            _sessionMock.Setup(s => s.TryGetValue(key, out bytes)).Returns(true);

            var result = _sessionProvider.GetSessionValue(key);

            Assert.Equal(value, result);
        }

        [Fact]
        public void RemoveSessionValue_RemovesCorrectKey()
        {
            var key = "TestKey";

            _sessionProvider.RemoveSessionValue(key);

            _sessionMock.Verify(s => s.Remove(key), Times.Once);
        }

        [Fact]
        public void ContainsSessionKey_ReturnsTrue_IfKeyExists()
        {
            var key = "TestKey";
            _sessionMock.Setup(s => s.Keys).Returns(new List<string> { "TestKey" });

            var result = _sessionProvider.ContainsSessionKey(key);

            Assert.True(result);
        }

        [Fact]
        public void ContainsSessionKey_ReturnsFalse_IfKeyDoesNotExist()
        {
            var key = "MissingKey";
            _sessionMock.Setup(s => s.Keys).Returns(new List<string> { "TestKey" });

            var result = _sessionProvider.ContainsSessionKey(key);

            Assert.False(result);
        }

        [Fact]
        public void ClearSession_RemovesAllKeys()
        {
            var keys = new List<string> { "Key1", "Key2" };
            _sessionMock.Setup(s => s.Keys).Returns(keys);

            _sessionProvider.ClearSession();

            foreach (var key in keys)
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
            var provider = new SessionProvider(_httpContextAccessorMock.Object);

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
            var key = "obj";
            var obj = new TestObject { Name = "John", Age = 30 };
            var json = JsonSerializer.Serialize(obj);
            var expectedBytes = Encoding.UTF8.GetBytes(json);

            _sessionMock.Setup(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))));

            _sessionProvider.SetSessionObject(key, obj);

            _sessionMock.Verify(s => s.Set(key, It.Is<byte[]>(b => b.SequenceEqual(expectedBytes))), Times.Once);
        }

        [Fact]
        public void GetSessionObject_DeserializesStoredJson()
        {
            var key = "obj";
            var obj = new TestObject { Name = "Jane", Age = 25 };
            var json = JsonSerializer.Serialize(obj);
            var bytes = Encoding.UTF8.GetBytes(json);

            _sessionMock.Setup(s => s.TryGetValue(key, out bytes)).Returns(true);

            var result = _sessionProvider.GetSessionObject<TestObject>(key);

            Assert.Equal(obj, result);
        }

        [Fact]
        public void GetSessionObject_ReturnsDefault_WhenKeyNotFound()
        {
            var key = "missing";
            byte[] outBytes = null;

            _sessionMock.Setup(s => s.TryGetValue(key, out outBytes)).Returns(false);

            var result = _sessionProvider.GetSessionObject<TestObject>(key);

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
}
