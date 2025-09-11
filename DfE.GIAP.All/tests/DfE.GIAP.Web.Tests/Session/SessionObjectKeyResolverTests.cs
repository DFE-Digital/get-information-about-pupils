﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Web.Session.Infrastructure;
using Xunit;

namespace DfE.GIAP.Web.Tests.Session;
public sealed class SessionObjectKeyResolverTests
{
    [Theory]
    [InlineData(typeof(NestedSessionObject), "DfE.GIAP.Web.Tests.Session.SessionObjectKeyResolverTests+NestedSessionObject")]
    [InlineData(typeof(SessionObject), "DfE.GIAP.Web.Tests.Session.SessionObject")]
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
        string expectedKey = "DfE.GIAP.Web.Tests.Session.SessionObject";

        SessionObjectKeyResolver sut = new();

        // Act
        string output = sut.Resolve<SessionObject>();

        Assert.Equal(expectedKey, output);
    }

    public class NestedSessionObject { }
}

public class SessionObject { }
