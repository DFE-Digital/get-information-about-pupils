using DfE.GIAP.Core.MyPupils.Domain;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Write;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;

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

    [Fact]
    public void Map_Aggregate_Mapped_To_DocumentDto()
    {
        // Arrange
        MyPupilsAggregate aggregate = MyPupilsAggregateTestDoubles.CreateWithSomePupils();

        MyPupilsAggregateToMyPupilsDocumentDtoMapper sut = new();

        // Act
        MyPupilsDocumentDto response = sut.Map(aggregate);

        // Arrange
        Assert.NotNull(response);
        Assert.Equal(aggregate.AggregateId.Value, response.id);

        IEnumerable<string> pupilUpns = aggregate.GetMyPupils().Select(t => t.Value);
        Assert.Equivalent(pupilUpns, response.MyPupils.Pupils.Select(t => t.UPN));
    }
}
