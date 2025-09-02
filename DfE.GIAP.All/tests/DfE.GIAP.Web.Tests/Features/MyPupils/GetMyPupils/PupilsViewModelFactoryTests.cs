using DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.ViewModel.Factory;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils;
public sealed class PupilsViewModelFactoryTests
{
    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Func<PupilViewModelFactory> construct = () => new(null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }
}
