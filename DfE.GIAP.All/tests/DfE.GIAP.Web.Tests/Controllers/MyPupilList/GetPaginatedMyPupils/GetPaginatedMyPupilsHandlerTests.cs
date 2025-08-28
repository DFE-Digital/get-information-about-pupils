using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Core.User.Application;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.PresentationState;
using DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.GetPaginatedMyPupils;
public sealed class GetPaginatedMyPupilsHandlerTests
{

    [Fact]
    public void Constructor_Throws_When_UseCase_Is_Null()
    {
        // Arrange
        Mock<IPupilDtosPresentationHandler> mockHandler = new();
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
        UserId userId = UserIdTestDoubles.Default();

        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IPupilDtosPresentationHandler> mockHandler = new();
        
        GetPaginatedMyPupilsHandler sut = new(
            useCaseMock.Object,
            mockHandler.Object);

        PupilDtos stubPupilDtos = PupilDtoTestDoubles.Generate(count: 20);
        MyPupilsPresentationState stubPupilsPresentationState = MyPupilsPresentationStateTestDoubles.CreateWithValidPage();

        useCaseMock
            .Setup(useCase => useCase.HandleRequestAsync(
                It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(new GetMyPupilsResponse(stubPupilDtos))
            .Verifiable();

        mockHandler
            .Setup(t => t.Handle(
                It.IsAny<PupilDtos>(),
                It.IsAny<MyPupilsPresentationState>()))
            .Returns(stubPupilDtos)
            .Verifiable();

        PupilDtos response =
            await sut.HandleAsync(
                new GetPaginatedMyPupilsRequest(
                    UserId: userId.Value,
                    PresentationState: stubPupilsPresentationState));

        Assert.NotNull(response);
        Assert.Equal(response, stubPupilDtos);

        useCaseMock.Verify(useCase => useCase.HandleRequestAsync(new GetMyPupilsRequest(userId.Value)), Times.Once);
        mockHandler.Verify(t => t.Handle(stubPupilDtos, stubPupilsPresentationState), Times.Once);
    }
}
