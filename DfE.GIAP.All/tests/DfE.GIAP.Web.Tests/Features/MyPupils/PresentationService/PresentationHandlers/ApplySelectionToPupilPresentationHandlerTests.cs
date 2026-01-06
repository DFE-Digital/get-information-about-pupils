using DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.SelectionState;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.PresentationService.PresentationHandlers;
public sealed class ApplySelectionToPupilPresentationHandlerTests
{
    [Fact]
    public void Handle_With_Null_Pupils_Returns_Empty_Pupils()
    {
        // Arrange
        ApplySelectionToPupilPresentationHandler sut = new();

        // Act
        MyPupilsPresentationPupilModels response = sut.Handle(
            null!, It.IsAny<MyPupilsPresentationQueryModel>(), It.IsAny<MyPupilsPupilSelectionState>());

        // Assert
        Assert.NotNull(response);
        Assert.Empty(response.Values);
    }
}
