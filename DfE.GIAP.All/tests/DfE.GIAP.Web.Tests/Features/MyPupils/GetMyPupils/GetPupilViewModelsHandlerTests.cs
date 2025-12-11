using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.GetMyPupils.Mapper;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.Models;
using DfE.GIAP.Web.Features.MyPupils.PresentationService.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State.Models;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.GetPupilViewModels;
public sealed class GetPupilViewModelsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_UseCase_Is_Null()
    {
        // Arrange
        Mock<IMyPupilsPresentationModelHandler> presentationHandlerMock = new();
        Mock<IMapper<PupilsSelectionContext, MyPupilsPresentationPupilModels>> mapperMock = MapperTestDoubles.Default<PupilsSelectionContext, MyPupilsPresentationPupilModels>();
        Func<GetMyPupilsHandler> construct = () => new(null, presentationHandlerMock.Object, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Handler_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMapper<PupilsSelectionContext, MyPupilsPresentationPupilModels>> mapperMock = MapperTestDoubles.Default<PupilsSelectionContext, MyPupilsPresentationPupilModels>();
        Func<GetMyPupilsHandler> construct = () => new(useCaseMock.Object, null, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMyPupilsPresentationModelHandler> presentationHandlerMock = new();
        Func<GetMyPupilsHandler> construct = () => new(useCaseMock.Object, presentationHandlerMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMyPupilsPresentationModelHandler> presentationHandlerMock = new();
        Mock<IMapper<PupilsSelectionContext, MyPupilsPresentationPupilModels>> mapperMock = MapperTestDoubles.Default<PupilsSelectionContext, MyPupilsPresentationPupilModels>();

        GetMyPupilsHandler sut = new(useCaseMock.Object, presentationHandlerMock.Object, mapperMock.Object);

        Func<Task> act = async () => await sut.GetPupilsAsync(null);

        // Act Assert
        await Assert.ThrowsAsync<ArgumentNullException>(act);
    }

    [Fact]
    public async Task GetPupilsAsync_Maps_Out_Pupils()
    {
        // Arrange

        MyPupilsModel stubMyPupilsModel = MyPupilDtosTestDoubles.Generate(count: 1);
        GetMyPupilsResponse useCaseResponseStub = new(stubMyPupilsModel);
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();

        useCaseMock
            .Setup((useCase) => useCase.HandleRequestAsync(It.IsAny<GetMyPupilsRequest>()))
            .ReturnsAsync(useCaseResponseStub);

        MyPupilsPresentationPupilModels pupilViewModelsStub = MyPupilsPresentationModelTestDoubles.Generate(count: 10);

        Mock<IMyPupilsPresentationModelHandler> presentationHandlerMock = new();
        presentationHandlerMock
            .Setup((handler) => handler.Handle(stubMyPupilsModel, It.IsAny<MyPupilsPresentationQueryModel>()))
            .Returns(stubMyPupilsModel);

        Mock<IMapper<PupilsSelectionContext, MyPupilsPresentationPupilModels>> mapperMock =
            MapperTestDoubles.MockFor<PupilsSelectionContext, MyPupilsPresentationPupilModels>(pupilViewModelsStub);

        GetMyPupilsHandler sut = new(
            useCaseMock.Object, presentationHandlerMock.Object, mapperMock.Object);

        MyPupilsState state = MyPupilsStateTestDoubles.Default();

        MyPupilsRequest request = new("id", state);

        // Act
        MyPupilsResponse response = await sut.GetPupilsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.MyPupils.Count);
        Assert.Equivalent(pupilViewModelsStub.Values, response.MyPupils);

        useCaseMock.Verify(u => u.HandleRequestAsync(It.Is<GetMyPupilsRequest>(r => r.UserId.Equals("id"))), Times.Once);

        presentationHandlerMock.Verify(p => p.Handle(stubMyPupilsModel, state.PresentationState), Times.Once);

        mapperMock
            .Verify((mapper) => mapper.Map(It.Is<PupilsSelectionContext>(ctx =>
                ctx.Pupils.Values.SequenceEqual(stubMyPupilsModel.Values) &&
                    ctx.SelectionState == state.SelectionState)),
                        Times.Once);
    }

}
