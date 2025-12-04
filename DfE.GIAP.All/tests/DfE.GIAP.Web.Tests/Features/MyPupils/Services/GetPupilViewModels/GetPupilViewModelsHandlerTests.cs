using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.MyPupils;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser;
using DfE.GIAP.Web.Features.MyPupils.Services.GetMyPupilsForUser.Mapper;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels;
using DfE.GIAP.Web.Features.MyPupils.Services.GetPupilViewModels.Handlers.PresentationHandlers;
using DfE.GIAP.Web.Features.MyPupils.State;
using DfE.GIAP.Web.Features.MyPupils.State.Presentation;
using DfE.GIAP.Web.Tests.TestDoubles.MyPupils;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Services.GetPupilViewModels;
public sealed class GetPupilViewModelsHandlerTests
{
    [Fact]
    public void Constructor_Throws_When_UseCase_Is_Null()
    {
        // Arrange
        Mock<IMyPupilDtosPresentationHandler> presentationHandlerMock = new();
        Mock<IMapper<PupilsSelectionContext, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<PupilsSelectionContext, PupilsViewModel>();
        Func<GetPupilViewModelsHandler> construct = () => new(null, presentationHandlerMock.Object, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Handler_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMapper<PupilsSelectionContext, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<PupilsSelectionContext, PupilsViewModel>();
        Func<GetPupilViewModelsHandler> construct = () => new(useCaseMock.Object, null, mapperMock.Object);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_Mapper_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMyPupilDtosPresentationHandler> presentationHandlerMock = new();
        Func<GetPupilViewModelsHandler> construct = () => new(useCaseMock.Object, presentationHandlerMock.Object, null);

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task HandleAsync_Throws_When_Request_Is_Null()
    {
        // Arrange
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IMyPupilDtosPresentationHandler> presentationHandlerMock = new();
        Mock<IMapper<PupilsSelectionContext, PupilsViewModel>> mapperMock = MapperTestDoubles.Default<PupilsSelectionContext, PupilsViewModel>();

        GetPupilViewModelsHandler sut = new(useCaseMock.Object, presentationHandlerMock.Object, mapperMock.Object);

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

        PupilsViewModel pupilViewModelsStub = PupilsViewModelTestDoubles.Generate(count: 10);

        Mock<IMyPupilDtosPresentationHandler> presentationHandlerMock = new();
        presentationHandlerMock
            .Setup((handler) => handler.Handle(stubMyPupilsModel, It.IsAny<MyPupilsPresentationState>()))
            .Returns(stubMyPupilsModel);

        Mock<IMapper<PupilsSelectionContext, PupilsViewModel>> mapperMock =
            MapperTestDoubles.MockFor<PupilsSelectionContext, PupilsViewModel>(pupilViewModelsStub);

        GetPupilViewModelsHandler sut = new(
            useCaseMock.Object, presentationHandlerMock.Object, mapperMock.Object);

        MyPupilsState state = MyPupilsStateTestDoubles.Default();

        GetPupilViewModelsRequest request = new("id", state);

        // Act
        PupilsViewModel response = await sut.GetPupilsAsync(request);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(10, response.Pupils.Count);
        Assert.Equivalent(pupilViewModelsStub.Pupils, response.Pupils);

        useCaseMock.Verify(u => u.HandleRequestAsync(It.Is<GetMyPupilsRequest>(r => r.MyPupilsId.Equals("id"))), Times.Once);

        presentationHandlerMock.Verify(p => p.Handle(stubMyPupilsModel, state.PresentationState), Times.Once);

        mapperMock
            .Verify((mapper) => mapper.Map(It.Is<PupilsSelectionContext>(ctx =>
                ctx.Pupils.Values.SequenceEqual(stubMyPupilsModel.Values) &&
                    ctx.SelectionState == state.SelectionState)),
                        Times.Once);
    }

}
