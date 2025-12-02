using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetMyPupils.GetPaginatedMyPupils;
public sealed class GetPaginatedMyPupilsHandlerTests
{

    [Fact]
    public void Constructor_Throws_When_UseCase_Is_Null()
    {
        // Arrange
        Mock<IMyPupilDtosPresentationHandler> mockHandler = new();
        Func<GetPaginatedMyPupilsHandler> construct = () => new(null, mockHandler.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PresentationHandler_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Func<GetPaginatedMyPupilsHandler> construct = () => new(useCaseMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Calls_UseCaseOnce_And_PresentationHandlerOnce_And_Returns_Result()
    {
        // Arrange
        MyPupilsId myPupilsId = MyPupilsIdTestDoubles.Default();
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMyPupilDtosPresentationHandler> mockHandler = new();

        MyPupilsModel stubPupilDtos = MyPupilDtosTestDoubles.Generate(count: 20);
        MyPupilsPresentationState stubPupilsPresentationState = MyPupilsPresentationStateTestDoubles.Default();

        useCaseMock
            .Setup(useCase => useCase.HandleRequestAsync(
                It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(new GetMyPupilsResponse(stubPupilDtos))
            .Verifiable();

        mockHandler
            .Setup(t => t.Handle(
                It.IsAny<MyPupilsModel>(),
                It.IsAny<MyPupilsPresentationState>()))
            .Returns(stubPupilDtos)
            .Verifiable();

        // Act
        GetPaginatedMyPupilsHandler sut = new(
            useCaseMock.Object,
            mockHandler.Object);

        PaginatedMyPupilsResponse response =
            await sut.HandleAsync(
                new GetPaginatedMyPupilsRequest(
                    MyPupilsId: myPupilsId.Value,
                    PresentationState: stubPupilsPresentationState));

        // Assert
        Assert.NotNull(response);
        Assert.Equal(response.Pupils, stubPupilDtos);

        useCaseMock.Verify(useCase => useCase.HandleRequestAsync(new GetMyPupilsRequest(myPupilsId.Value)), Times.Once);
        mockHandler.Verify(t => t.Handle(stubPupilDtos, stubPupilsPresentationState), Times.Once);
    }
}
