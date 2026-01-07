using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Messaging;
using DfE.GIAP.Web.Features.MyPupils.Messaging.DataTransferObjects;
using DfE.GIAP.Web.Shared.TempData;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Messaging;
public sealed class MyPupilsTempDataMessageSinkTests
{

    [Fact]
    public void Constructor_Throws_When_ToDataTransferObjectMapper_Null()
    {
        // Arrange
        Func<MyPupilsTempDataMessageSink> construct = () => new(
            null!,
            MapperTestDoubles.Default<MyPupilsMessageDto, MyPupilsMessage>().Object,
            OptionsTestDoubles.Default<MyPupilsMessagingOptions>(),
            new Mock<ITempDataDictionaryProvider>().Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }
}
