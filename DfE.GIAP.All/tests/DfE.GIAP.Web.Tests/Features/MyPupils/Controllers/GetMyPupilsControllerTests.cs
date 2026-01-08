using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.SharedTests.Common;
using DfE.GIAP.SharedTests.Runtime.TestDoubles;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Features.MyPupils.Controllers;
using DfE.GIAP.Web.Features.MyPupils.Controllers.GetMyPupils;
using DfE.GIAP.Web.Features.MyPupils.PresentationService;
using DfE.GIAP.Web.Tests.Features.MyPupils.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Features.MyPupils.Controllers;
public sealed class GetMyPupilsControllerTests
{
    [Fact]
    public void Constructor_Throws_When_Logger_Is_Null()
    {
        // Arrange
        Func<GetMyPupilsController> construct = () => new(
            logger: null,
            IMyPupilsPresentationServiceTestDoubles.DefaultMock().Object,
            MapperTestDoubles.Default<MyPupilsPresentationResponse, MyPupilsViewModel>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_PresentationService_Is_Null()
    {
        // Arrange
        Func<GetMyPupilsController> construct = () => new(
            LoggerTestDoubles.Fake<GetMyPupilsController>(),
            null!,
            MapperTestDoubles.Default<MyPupilsPresentationResponse, MyPupilsViewModel>().Object
        );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public void Constructor_Throws_When_StateProvider_Is_Null()
    {
        // Arrange       
        Func<GetMyPupilsController> construct = () => new(
            LoggerTestDoubles.Fake<GetMyPupilsController>(),
            IMyPupilsPresentationServiceTestDoubles.DefaultMock().Object,
            null
            );

        // Act Assert
        Assert.Throws<ArgumentNullException>(construct);
    }

    [Fact]
    public async Task Index_Returns_PupilViewModels()
    {
        // Arrange
        InMemoryLogger<GetMyPupilsController> loggerFake = LoggerTestDoubles.Fake<GetMyPupilsController>();

        MyPupilsPresentationPupilModels pupilsStub = MyPupilsPresentationPupilModelsTestDoubles.Generate(count: 10);

        MyPupilsPresentationResponse presentationResponseStub = new()
        {
            MyPupils = pupilsStub,
            PageNumber = 1,
            TotalPages = 100,
            IsAnyPupilsSelected = false,
        };

        IMyPupilsPresentationService serviceMock =
            IMyPupilsPresentationServiceTestDoubles.MockForGetPupils(presentationResponseStub);

        Mock<IMapper<MyPupilsPresentationResponse, MyPupilsViewModel>> mapperMock =
            MapperTestDoubles.MockFor<MyPupilsPresentationResponse, MyPupilsViewModel>(
                new MyPupilsViewModel());

        GetMyPupilsController sut = new(
            loggerFake,
            serviceMock,
            mapperMock.Object);

        HttpContext context = sut.StubHttpContext();
        sut.StubTempData([], context);

        MyPupilsQueryRequestDto requestQueryDto = new();

        // Act
        IActionResult actionResult = await sut.Index(requestQueryDto);

        // Assert

        ViewResult viewResult = Assert.IsType<ViewResult>(actionResult);
        Assert.NotNull(viewResult);

        Assert.Equal("~/Views/MyPupilList/Index.cshtml", viewResult.ViewName);

        MyPupilsViewModel viewModel = Assert.IsType<MyPupilsViewModel>(viewResult.Model);
        Assert.NotNull(viewModel);

        Assert.Equal("GetMyPupilsController.Index GET called", loggerFake.Logs.Single());

        Mock.Get(serviceMock)
            .Verify((service)
                => service.GetPupilsAsync(
                    context.User.GetUserId(), // TODO need to control ClaimsPrincipal in Stub
                        requestQueryDto), Times.Once);

        mapperMock.Verify(mapper => mapper.Map(
            It.Is<MyPupilsPresentationResponse>(
                presentationResponse => presentationResponse.Equals(presentationResponseStub))), Times.Once);
    }
}
