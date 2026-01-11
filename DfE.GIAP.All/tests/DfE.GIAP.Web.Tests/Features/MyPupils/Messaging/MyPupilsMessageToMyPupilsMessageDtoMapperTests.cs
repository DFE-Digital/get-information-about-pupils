using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Messaging;
public sealed class MyPupilsMessageToMyPupilsMessageDtoMapperTests
{
    [Fact]
    public void Map_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilsMessageToMyPupilsMessageDtoMapper sut = new();

        // Act
        Func<MyPupilsMessageDto> act = () => sut.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Maps_Message_To_MessageDto()
    {
        // Arrange
        MyPupilsMessage input = MyPupilsMessage.Create("id", MessageLevel.Debug, "Test message");

        MyPupilsMessageToMyPupilsMessageDtoMapper sut = new();

        // Act
        MyPupilsMessageDto response = sut.Map(input);

        Assert.Equal(input.Id, response.Id);
        Assert.Equal(input.Message, response.Message);
        Assert.Equal(input.Level, response.MessageLevel);
    }
}
