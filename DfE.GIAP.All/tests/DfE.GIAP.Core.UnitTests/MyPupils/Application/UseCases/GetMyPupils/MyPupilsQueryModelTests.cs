using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using Xunit;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.UseCases.GetMyPupils;
public sealed class MyPupilsQueryModelTests
{
    [Fact]
    public void Handle_Throws_When_PageNumber_Is_LessThan_1()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => MyPupilsQueryModelTestDoubles.Create(page: 0));
    }
}
