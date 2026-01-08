using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.Models;
public sealed class MyPupilsPresentationQueryModelTests
{
    [Fact]
    public void Handle_Throws_When_PageNumber_Is_LessThan_1()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MyPupilsPresentationQueryModelTestDoubles.Create(page: 0));
    }
}
