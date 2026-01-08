using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class MyPupilsAggregateToMyPupilsDocumentDtoMapperTests
{
    [Fact]
    public void Map_Null_Throws_ArgumentNullException()
    {
        // Arrange
        MyPupilsAggregateToMyPupilsDocumentDtoMapper sut = new();

        Func<MyPupilsDocumentDto> act = () => sut.Map(null!);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(act);
    }
}
