using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.Messaging.Mapper;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Messaging.Mapper;
public sealed class MyPupilsMessageDtoToMyPupilsMessageMapperTests
{
    [Fact]
    public void Handle_Throws_When_Input_Is_Null()
    {
        // Arrange
        MyPupilsMessageDtoToMyPupilsMessageMapper sut = new();

        // Act
        Func<MyPupilsMessage> act = () => sut.Map(null!);

        // Assert
        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_Maps_MessageDto_To_Message()
    {
        // Arrange
        MyPupilsMessageDto input = new()
        {
            Id = "id",
            MessageLevel = MessageLevel.Info,
            Message = "Test message"
        };

        MyPupilsMessageDtoToMyPupilsMessageMapper sut = new();

        // Act
        MyPupilsMessage response = sut.Map(input);

        Assert.Equal(input.Id, response.Id);
        Assert.Equal(input.Message, response.Message);
        Assert.Equal(input.MessageLevel, response.Level);
    }
}