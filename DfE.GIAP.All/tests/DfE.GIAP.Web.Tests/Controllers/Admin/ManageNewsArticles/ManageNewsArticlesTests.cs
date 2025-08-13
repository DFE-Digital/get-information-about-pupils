using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Web.Controllers.Admin.ManageNewsArticles;
using DfE.GIAP.Web.ViewModels.Admin.ManageNewsArticles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Admin.ManageNewsArticles;
public class ManageNewsArticlesControllerTests
{
    private readonly Mock<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>> _getByIdMock = new();
    private readonly Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> _getAllMock = new();
    private readonly Mock<IUseCaseRequestOnly<DeleteNewsArticleRequest>> _deleteMock = new();
    private readonly Mock<IUseCaseRequestOnly<CreateNewsArticleRequest>> _createMock = new();
    private readonly Mock<IUseCaseRequestOnly<UpdateNewsArticleRequest>> _updateMock = new();
    private readonly Mock<ITextSanitiserInvoker> _sanitiserMock = new();

    private ManageNewsArticlesController CreateController()
    {
        ManageNewsArticlesController controller = new(
            _getByIdMock.Object,
            _getAllMock.Object,
            _deleteMock.Object,
            _createMock.Object,
            _updateMock.Object,
            _sanitiserMock.Object);

        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        return controller;
    }

    [Fact]
    public void CreateNewsArticle_Get_ReturnsViewWithModel()
    {
        ManageNewsArticlesController controller = CreateController();

        IActionResult result = controller.CreateNewsArticle();

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/CreateNewsArticle", viewResult.ViewName);
        Assert.IsType<CreateNewsArticleViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task CreateNewsArticle_Post_InvalidModel_ReturnsSameView()
    {
        ManageNewsArticlesController controller = CreateController();
        controller.ModelState.AddModelError("Title", "Required");

        CreateNewsArticleViewModel model = new();

        IActionResult result = await controller.CreateNewsArticle(model);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/CreateNewsArticle", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task CreateNewsArticle_Post_ValidModel_CallsUseCaseAndReturnsConfirmation()
    {
        ManageNewsArticlesController controller = CreateController();
        CreateNewsArticleViewModel model = new()
        {
            NewsArticle = new()
            {
                Title = "Test Title",
                Body = "Test Body",
                Published = true,
                Pinned = false
            }
        };

        IActionResult result = await controller.CreateNewsArticle(model);

        _createMock.Verify(x => x.HandleRequestAsync(It.IsAny<CreateNewsArticleRequest>()), Times.Once);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/NewsArticleConfirmation", viewResult.ViewName);
        Assert.IsType<ConfirmationViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task DeleteNewsArticle_ValidId_CallsUseCaseAndReturnsConfirmationView()
    {
        ManageNewsArticlesController controller = CreateController();
        ManageNewsArticlesViewModel model = new() { SelectedNewsId = "123" };

        IActionResult result = await controller.DeleteNewsArticle(model);

        _deleteMock.Verify(x => x.HandleRequestAsync(It.IsAny<DeleteNewsArticleRequest>()), Times.Once);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/NewsArticleConfirmation", viewResult.ViewName);
    }

    [Fact]
    public async Task UpdateNewsArticle_InvalidModel_ReturnsSameView()
    {
        ManageNewsArticlesController controller = CreateController();
        controller.ModelState.AddModelError("Title", "Required");

        EditNewsArticleViewModel model = new();

        IActionResult result = await controller.UpdateNewsArticle(model);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/EditNewsArticle", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task UpdateNewsArticle_ValidModel_CallsUseCaseAndReturnsConfirmation()
    {
        ManageNewsArticlesController controller = CreateController();
        EditNewsArticleViewModel model = new()
        {
            NewsArticle = new NewsArticleViewModel
            {
                Id = "123",
                Title = "Title",
                Body = "Body",
                Published = true,
                Pinned = false
            }
        };

        _sanitiserMock.Setup(x => x.Sanitise(It.IsAny<string>())).Returns(It.IsAny<SanitisedTextResult>());

        IActionResult result = await controller.UpdateNewsArticle(model);

        _updateMock.Verify(x => x.HandleRequestAsync(It.IsAny<UpdateNewsArticleRequest>()), Times.Once);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/NewsArticleConfirmation", viewResult.ViewName);
    }
}
