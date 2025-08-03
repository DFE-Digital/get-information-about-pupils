using DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Application.Services;
public sealed class TempAggregatePupilsForMyPupilsServiceTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(50, 1)]
    [InlineData(51, 1)]
    [InlineData(500, 1)]
    [InlineData(501, 2)]
    [InlineData(1000, 2)]
    [InlineData(1001, 3)]
    [InlineData(3999, 8)]
    [InlineData(4000, 8)]
    public void SplitUpnsToFitPagingLimit_SplitsCorrectly(int upnCount, int expectedChunks)
    {
        // Arrange
        List<UniquePupilNumber> upns = UniquePupilNumberTestDoubles.Generate(upnCount);

        // Act
        List<IEnumerable<string>> result = TempAggregatePupilsForMyPupilsApplicationService.SplitUpnsToFitPagingLimit(upns);

        // Assert
        Assert.Equal(expectedChunks, result.Count);
        Assert.Equal(upnCount, result.SelectMany(x => x).Count());
    }
}
