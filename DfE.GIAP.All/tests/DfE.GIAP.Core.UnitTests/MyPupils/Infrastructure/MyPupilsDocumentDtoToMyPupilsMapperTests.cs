using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.Read.Mapper;
using DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.MyPupils.Infrastructure;
public sealed class MyPupilsDocumentDtoToMyPupilsMapperTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilsDocumentDtoToMyPupilsMapper sut = new();

        // Act
        Action act = () => sut.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_DefaultsToEmptyMyPupils_When_MyPupils_Is_Null()
    {
        // Arrange
        MyPupilsDocumentDtoToMyPupilsMapper sut = new();

        MyPupilsDocumentDto myPupilsDocumentDto = new()
        {
            id = "stub-id",
            MyPupils = null
        };

        // Act
        Core.MyPupils.Application.Repositories.MyPupils response = sut.Map(myPupilsDocumentDto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal([], response.Pupils.GetUniquePupilNumbers());
    }

    [Fact]
    public void Map_DefaultsToEmptyMyPupils_When_Pupils_Is_Null()
    {
        // Arrange
        MyPupilsDocumentDtoToMyPupilsMapper sut = new();

        MyPupilsDocumentDto myPupilsDocumentDto = new()
        {
            id = "stub-id",
            MyPupils = new()
            {
                Pupils = null!
            }
        };

        // Act
        Core.MyPupils.Application.Repositories.MyPupils response = sut.Map(myPupilsDocumentDto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal([], response.Pupils.GetUniquePupilNumbers());
    }

    [Fact]
    public void Map_DefaultsToEmptyMyPupils_When_Pupils_Is_Empty()
    {
        // Arrange
        MyPupilsDocumentDtoToMyPupilsMapper sut = new();

        MyPupilsDocumentDto myPupilsDocumentDto = new()
        {
            id = "stub-id",
            MyPupils = new()
            {
                Pupils = []
            }
        };

        // Act
        Core.MyPupils.Application.Repositories.MyPupils response = sut.Map(myPupilsDocumentDto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal([], response.Pupils.GetUniquePupilNumbers());
    }

    [Fact]
    public void Map_Maps_PupilUpns()
    {
        // Arrange
        MyPupilsDocumentDtoToMyPupilsMapper sut = new();

        List<UniquePupilNumber> generatedUpns = UniquePupilNumberTestDoubles.Generate(count: 10);

        MyPupilsDocumentDto myPupilsDocumentDto = MyPupilsDocumentDtoTestDoubles.Create(
            userId: UserIdTestDoubles.Default(),
            upns: UniquePupilNumbers.Create(generatedUpns));

        // Act
        Core.MyPupils.Application.Repositories.MyPupils response = sut.Map(myPupilsDocumentDto);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(generatedUpns.Count, response.Pupils.Count);
        Assert.Equal(generatedUpns, response.Pupils.GetUniquePupilNumbers());
    }
}
