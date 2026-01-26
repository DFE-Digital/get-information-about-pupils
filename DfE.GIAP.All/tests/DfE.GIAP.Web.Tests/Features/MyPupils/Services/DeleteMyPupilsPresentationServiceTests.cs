using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.DeletePupilsFromMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.DeletePupils;
using DfE.GIAP.Web.Features.MyPupils.PupilSelection.ClearPupilSelections;
using DfE.GIAP.Web.Features.MyPupils.Services.GetSelectedPupilIdentifiers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services;
public sealed class DeleteMyPupilsPresentationServiceTests
{
    [Theory]
    [MemberData(nameof(DeletePupilsInput))]
    public async Task DeletePupilsAsync_CallsUseCase_And_ClearsPupilSelections(
       List<string>? requestSelectedPupils,
       List<string> alreadySelectedPupils,
       List<string> expectedDeletePupils)
    {
        // Arrange
        Mock<IUseCaseRequestOnly<DeletePupilsFromMyPupilsRequest>> useCaseMock = new();

        Mock<IClearMyPupilsPupilSelectionsHandler> clearPupilSelectionsHandlerMock = new();

        Mock<IGetSelectedPupilsUniquePupilNumbersPresentationService> getSelectionProviderMock = new();

        getSelectionProviderMock
            .Setup(t => t.GetSelectedPupilsAsync(It.IsAny<string>()))
            .ReturnsAsync(alreadySelectedPupils);

        const string userId = "userId";

        DeleteMyPupilsPresentationService sut = new(
            deletePupilsUseCase: useCaseMock.Object,
            getSelectionsHandler: getSelectionProviderMock.Object,
            clearPupilSelectionsHandlerMock.Object);

        // Act
        await sut.DeletePupilsAsync(userId, requestSelectedPupils);

        // Assert
        getSelectionProviderMock.Verify(
            (provider) => provider.GetSelectedPupilsAsync(It.Is<string>(id => userId == id)), Times.Once);

        useCaseMock.Verify((useCase) =>
            useCase.HandleRequestAsync(
                It.Is<DeletePupilsFromMyPupilsRequest>(
                    (request) => request.UserId.Equals(userId) &&
                        request.DeletePupilUpns.SequenceEqual(expectedDeletePupils))), Times.Once());

        clearPupilSelectionsHandlerMock.Verify(
            (handler) => handler.Handle(), Times.Once);
    }

    public static TheoryData<List<string>?, List<string>, List<string>> DeletePupilsInput
    {
        get
        {
            return new TheoryData<List<string>?, List<string>, List<string>>
            {
                // null selections is defaulted
                {
                    null!,
                    [],
                    []
                },
                // [] is handled
                {
                    [],
                    [],
                    []
                },
                // selections are added from request to empty state
                {
                    ["1" ],
                    [],
                    ["1"]
                },
                // selections are appended from request to state
                {
                    ["1" ],
                    ["2"],
                    ["1", "2"]
                },
                // selections are deduplicated
                {
                    ["1" ],
                    ["1", "2"],
                    ["1", "2"]
                },
            };
        }
    }
}
