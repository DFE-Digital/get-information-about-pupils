﻿using DfE.GIAP.Web.Session.Abstraction.Command;
using Moq;

namespace DfE.GIAP.Web.Tests.TestDoubles.Session;
internal static class ISessionCommandHandlerTestDoubles
{
    internal static Mock<ISessionCommandHandler<TSessionObject>> Default<TSessionObject>() where TSessionObject : class
        => new();
}
