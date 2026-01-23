using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Handlers;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Invoker;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Content.Controllers;
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
    private readonly Mock<ITextSanitiser> _sanitiserMock = new();

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
    public void SelectNewsArticle_SetsTempDataAndRedirects()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();
        ManageNewsArticlesViewModel viewModel = new() { SelectedNewsId = "news-123" };

        // Act
        IActionResult result = controller.SelectNewsArticle(viewModel);

        // Assert
        Assert.Equal("news-123", controller.TempData["SelectedNewsId"]);
        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("EditNewsArticle", redirect.ActionName);
    }

    [Fact]
    public async Task ManageNewsArticles_ReturnsViewWithPopulatedModel()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();
        List<NewsArticle> articles = new()
        {
            new NewsArticle
            {
                Id = NewsArticleIdentifier.From("id1"),
                Title = "Title 1",
                Body = "Body 1",
                Published = true,
                Pinned = false,
                CreatedDate = DateTime.UtcNow.AddDays(-2),
                ModifiedDate = DateTime.UtcNow.AddDays(-1)
            },
            new NewsArticle
            {
                Id = NewsArticleIdentifier.From("id2"),
                Title = "Title 2",
                Body = "Body 2",
                Published = false,
                Pinned = true,
                CreatedDate = DateTime.UtcNow.AddDays(-3),
                ModifiedDate = DateTime.UtcNow
            }
        };

        GetNewsArticlesResponse response = new GetNewsArticlesResponse(articles);
        _getAllMock.Setup(x => x.HandleRequestAsync(It.IsAny<GetNewsArticlesRequest>()))
            .ReturnsAsync(response);

        // Act
        IActionResult result = await controller.ManageNewsArticles();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/ManageNewsArticles", viewResult.ViewName);

        ManageNewsArticlesViewModel model = Assert.IsType<ManageNewsArticlesViewModel>(viewResult.Model);
        Assert.Equal(string.Empty, model.SelectedNewsId);
        Assert.NotNull(model.NewsArticleList);
        Assert.Equal(2, model.NewsArticleList.Count());
        Assert.NotNull(model.BackButton);
        Assert.True(model.BackButton.IsBackButtonEnabled);
        Assert.Equal("Admin", model.BackButton.PreviousController);
        Assert.Equal("Index", model.BackButton.PreviousAction);
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

    [Fact]
    public async Task EditNewsArticle_NoSelectedNewsId_RedirectsToManageNewsArticles()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();

        // Act
        IActionResult result = await controller.EditNewsArticle();

        // Assert
        RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ManageNewsArticles", redirectResult.ActionName);
        Assert.Equal("ManageNewsArticles", redirectResult.ControllerName);
    }

    [Fact]
    public async Task EditNewsArticle_SelectedNewsId_CallsUseCaseAndReturnsViewWithModel()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();
        string newsId = "test-id";
        controller.TempData["SelectedNewsId"] = newsId;

        NewsArticle newsArticle = new NewsArticle
        {
            Id = NewsArticleIdentifier.From(newsId),
            Title = "Title",
            Body = "Body",
            CreatedDate = DateTime.UtcNow.AddDays(-1),
            ModifiedDate = DateTime.UtcNow,
            Published = true,
            Pinned = false
        };

        GetNewsArticleByIdResponse response = new GetNewsArticleByIdResponse(newsArticle);

        _getByIdMock
            .Setup(x => x.HandleRequestAsync(It.Is<GetNewsArticleByIdRequest>(r => r.Id.Value == newsId)))
            .ReturnsAsync(response);

        _sanitiserMock
            .Setup(x => x.Sanitise(It.IsAny<string>()))
            .Returns<string>(s => SanitisedTextResult.From(s));

        // Act
        IActionResult result = await controller.EditNewsArticle();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/EditNewsArticle", viewResult.ViewName);

        EditNewsArticleViewModel model = Assert.IsType<EditNewsArticleViewModel>(viewResult.Model);
        Assert.Equal(newsId, model.SelectedNewsId);
        Assert.NotNull(model.NewsArticle);
        Assert.Equal(newsId, model.NewsArticle.Id);
        Assert.Equal("Title", model.NewsArticle.Title);
        Assert.Equal("Body", model.NewsArticle.Body);
        Assert.True(model.NewsArticle.Published);
        Assert.False(model.NewsArticle.Pinned);
        Assert.Equal(newsArticle.CreatedDate, model.NewsArticle.CreatedDate);
        Assert.Equal(newsArticle.ModifiedDate, model.NewsArticle.ModifiedDate);

        Assert.NotNull(model.BackButton);
        Assert.True(model.BackButton.IsBackButtonEnabled);
        Assert.Equal("ManageNewsArticles", model.BackButton.PreviousController);
        Assert.Equal("ManageNewsArticles", model.BackButton.PreviousAction);
    }

    [Fact]
    public async Task EditNewsArticle_NullNewsArticle_ThrowsArgumentNullException()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();
        string newsId = "test-id";
        controller.TempData["SelectedNewsId"] = newsId;

        GetNewsArticleByIdResponse response = new GetNewsArticleByIdResponse(null);

        _getByIdMock
            .Setup(x => x.HandleRequestAsync(It.IsAny<GetNewsArticleByIdRequest>()))
            .ReturnsAsync(response);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() => controller.EditNewsArticle());
    }

    [Fact]
    public async Task EditNewsArticle_WithoutSelectedNewsId_RedirectsToManageNewsArticles()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();

        // Act
        IActionResult result = await controller.EditNewsArticle();

        // Assert
        RedirectToActionResult redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("ManageNewsArticles", redirect.ActionName);
        Assert.Equal("ManageNewsArticles", redirect.ControllerName);
    }

    [Fact]
    public async Task UpdateNewsArticle_InvalidModel_ReturnsEditViewWithSameModel()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();
        controller.ModelState.AddModelError("Title", "Required");
        EditNewsArticleViewModel model = new EditNewsArticleViewModel
        {
            NewsArticle = new NewsArticleViewModel()
        };

        // Act
        IActionResult result = await controller.UpdateNewsArticle(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/EditNewsArticle", viewResult.ViewName);
        Assert.Equal(model, viewResult.Model);
    }

    [Fact]
    public async Task UpdateNewsArticle_ValidModel_CallsUseCaseAndReturnsConfirmationView()
    {
        // Arrange
        ManageNewsArticlesController controller = CreateController();
        EditNewsArticleViewModel model = new EditNewsArticleViewModel
        {
            NewsArticle = new NewsArticleViewModel
            {
                Id = "id",
                Title = "Title",
                Body = "Body",
                Pinned = true,
                Published = false
            }
        };

        _sanitiserMock.Setup(x => x.Sanitise(It.IsAny<string>()))
            .Returns<string>(s => SanitisedTextResult.From(s));

        // Act
        IActionResult result = await controller.UpdateNewsArticle(model);

        // Assert
        _updateMock.Verify(x => x.HandleRequestAsync(It.IsAny<UpdateNewsArticleRequest>()), Times.Once);

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageNewsArticles/NewsArticleConfirmation", viewResult.ViewName);
        ConfirmationViewModel confirmation = Assert.IsType<ConfirmationViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Success.UpdateTitle, confirmation.Title);
        Assert.Equal(Messages.NewsArticle.Success.UpdateBody, confirmation.Body);
    }
}
