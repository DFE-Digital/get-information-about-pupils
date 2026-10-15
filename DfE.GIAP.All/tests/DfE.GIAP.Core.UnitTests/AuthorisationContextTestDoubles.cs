namespace DfE.GIAP.Core.UnitTests;
internal static class AuthorisationContextTestDoubles
{
    internal static IAuthorisationContext Create() => CreateMock().Object;
    internal static Mock<IAuthorisationContext> CreateMock() => new();
    internal static Mock<IAuthorisationContext> MockFor(string userId)
    {
        Mock<IAuthorisationContext> mock = CreateMock(); ;
        mock.Setup(t => t.UserId).Returns(userId).Verifiable();
        return mock;
    }
}
