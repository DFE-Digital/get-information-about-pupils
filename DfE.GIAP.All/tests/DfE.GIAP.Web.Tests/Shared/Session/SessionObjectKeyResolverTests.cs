using DfE.GIAP.Web.Shared.Session.Infrastructure;
using Xunit;

namespace DfE.GIAP.Web.Tests.Shared.Session;
public sealed class SessionObjectKeyResolverTests
{
    [Theory]
    [InlineData(typeof(NestedSessionObject), "DfE.GIAP.Web.Tests.Shared.Session.SessionObjectKeyResolverTests+NestedSessionObject")]
    [InlineData(typeof(SessionObject), "DfE.GIAP.Web.Tests.Shared.Session.SessionObject")]
    public void Resolves_Type_Returns_SessionKey_To_NamespaceAndType(Type type, string expectedKey)
    {
        SessionObjectKeyResolver sut = new();

        // Act
        string output = sut.Resolve(type);

        Assert.Equal(expectedKey, output);
    }

    [Fact]
    public void Resolves_GenericType_Returns_SessionKey_To_NamespaceAndType()
    {
        string expectedKey = "DfE.GIAP.Web.Tests.Shared.Session.SessionObject";

        SessionObjectKeyResolver sut = new();

        // Act
        string output = sut.Resolve<SessionObject>();

        Assert.Equal(expectedKey, output);
    }

    public class NestedSessionObject { }
}

public class SessionObject { }
