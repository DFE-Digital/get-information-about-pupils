using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants.Messages.Articles;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Models;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.ManageDocument;
using DfE.GIAP.Service.News;
using DfE.GIAP.Web.Controllers.Admin.ManageDocuments;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Admin;

[Trait("Category", "Manage Documents Controller Unit Tests")]
public class ManageDocumentsControllerTests : IClassFixture<UserClaimsPrincipalFake>, IClassFixture<ManageDocumentsResultsFake>
{
    private readonly Mock<IOptions<AzureAppSettings>> _mockAzureAppSettings = new Mock<IOptions<AzureAppSettings>>();
    private readonly Mock<ISession> _mockSession = new Mock<ISession>();
    private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
    private readonly ManageDocumentsResultsFake _manageDocumentsResultsFake;
    private readonly Mock<IContentService> _mockContentService = new();
    private readonly Mock<IManageDocumentsService> _mockDocRepo = new();
    private readonly Mock<INewsService> _mockNewsService = new();
    private readonly Mock<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>> _mockGetNewsArticleByIdUseCase = new();
    private readonly Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> _mockGetNewsArticlesUseCase = new();
    private readonly ILogger<ManageDocumentsController> _mockLogger = Substitute.For<ILogger<ManageDocumentsController>>();
    private readonly ICookieManager _mockCookieManager = Substitute.For<ICookieManager>();
    private readonly Mock<ICommonService> _commonService = new Mock<ICommonService>();
    private readonly ILatestNewsBanner _mockNewsBanner = Substitute.For<ILatestNewsBanner>();

    public ManageDocumentsControllerTests(UserClaimsPrincipalFake userClaimsPrincipalFake, ManageDocumentsResultsFake manageDocumentsResultsFake)
    {
        _userClaimsPrincipalFake = userClaimsPrincipalFake;
        _manageDocumentsResultsFake = manageDocumentsResultsFake;

        _mockAzureAppSettings.Setup(x => x.Value)
            .Returns(new AzureAppSettings() { IsSessionIdStoredInCookie = false });
        _mockSession.Setup(x => x.Id).Returns("12345");
    }

    [Fact]
    public async Task LoadListOfDocuments_When_ManageDocuments_MethodIsCalled()
    {
        // Arrange
        List<Document> expectedDocumentsList = new()
        {
            new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            new Document() { Id = 2, DocumentId = "PublicationSchedule", DocumentName = "Publication Schedule", SortId = 2, IsEnabled = true },
            new Document() { Id = 3, DocumentId = "PlannedMaintenance", DocumentName = "Planned Maintenance", SortId = 3, IsEnabled = true },
        };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(commonResponseBody);
        _mockDocRepo.Setup(repo => repo.GetDocumentsList()).Returns(expectedDocumentsList);

        var controller = GetManageDocumentsController();

        // Act
        var result = await controller.ManageDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(3, viewResult.ViewData.Values.Count);
        Assert.True(viewResult.ViewData.ContainsKey("IsSuccess"));
        Assert.True(viewResult.ViewData.ContainsKey("ListOfDocuments"));
    }

    [Fact]
    public async Task LoadNewsDocuments_When_ManageDocuments_Posted_MethodIsCalled_And_User_Has_Selected_News_article()
    {
        // Arrange
        List<Document> expectedDocumentsList = new()
        {
            new() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            new() { Id = 2, DocumentId = "PublicationSchedule", DocumentName = "Publication Schedule", SortId = 2, IsEnabled = true },
            new() { Id = 3, DocumentId = "PlannedMaintenance", DocumentName = "Planned Maintenance", SortId = 3, IsEnabled = true }
        };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(commonResponseBody);
        _mockDocRepo.Setup(repo => repo.GetDocumentsList()).Returns(expectedDocumentsList);

        var model = new ManageDocumentsViewModel { DocumentList = new Document { Id = 1, DocumentName = "Test title", DocumentId = "NewsArticle" } };

        var controller = GetManageDocumentsController();

        // Act
        var result = await controller.ManageDocuments(model, string.Empty, string.Empty).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(3, viewResult.ViewData.Values.Count);
        Assert.True(viewResult.ViewData.ContainsKey("IsSuccess"));
        Assert.True(viewResult.ViewData.ContainsKey("ListOfDocuments"));
    }

    [Fact]
    public async Task FailedToLoad_ListOfDocuments_WhenManageDocumentsMethodIsCalled()
    {
        // Arrange

        List<Document> expectedDocumentsList = new List<Document>();

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(commonResponseBody);
        _mockDocRepo.Setup(repo => repo.GetDocumentsList()).Returns(expectedDocumentsList);

        var controller = GetManageDocumentsController();

        // Act
        var result = await controller.ManageDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var isSuccess = Assert.IsAssignableFrom<Boolean>(viewResult.ViewData["IsSuccess"]);
        Assert.False(isSuccess);
    }

    [Fact]
    public async Task SelectedDocument_FromDropDownList_PostBackToManageDocumentsMethod()
    {
        // Arrange
        List<Document> documentsList = new List<Document>
        {
            new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            new Document() { Id = 2, DocumentId = "PublicationSchedule", DocumentName = "Publication Schedule", SortId = 2, IsEnabled = true },
            new Document() { Id = 3, DocumentId = "PlannedMaintenance", DocumentName = "Planned Maintenance", SortId = 3, IsEnabled = true }
        };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(commonResponseBody);
        _mockDocRepo.Setup(repo => repo.GetDocumentsList()).Returns(documentsList);

        var controller = GetManageDocumentsController();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();
        var discard = "Discard";
        var editDocument = "EditDocument";

        // Act
        var result = await controller.ManageDocuments(model, discard, editDocument).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).DocumentData;
        Assert.Equal(commonResponseBody.Id, documentData.Id);
        Assert.Equal(3, viewResult.ViewData.Values.Count);
        Assert.True(viewResult.ViewData.ContainsKey("IsSuccess"));
        Assert.True(viewResult.ViewData.ContainsKey("ListOfDocuments"));
    }

    [Fact]
    public async Task PublishChangesInDocument_PostBackMethod()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();

        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        List<Document> documentsList = new List<Document>
        {
            new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            new Document() { Id = 2, DocumentId = "PublicationSchedule", DocumentName = "Publication Schedule", SortId = 2, IsEnabled = true },
            new Document() { Id = 3, DocumentId = "PlannedMaintenance", DocumentName = "Planned Maintenance", SortId = 3, IsEnabled = true }
        };

        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockDocRepo.Setup(repo => repo.GetDocumentsList()).Returns(documentsList);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;

        var publish = "Publish";
        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Check_news_article_is_archived()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        // Act
        var result = await controller.ArchiveNews(model).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Confirmation", viewResult.ViewName);
        Assert.IsType<ManageDocumentsViewModel>(viewResult.Model);
        _mockContentService.Verify(repo => repo.SetDocumentToPublished(It.Is<CommonRequestBody>(x => x.Action == (int)ActionTypes.Archive), It.IsAny<AzureFunctionHeaderDetails>()));
    }

    [Fact]
    public async Task Check_news_article_is_unarchived()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        // Act
        var result = await controller.UnarchiveNews(model).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Confirmation", viewResult.ViewName);
        Assert.IsType<ManageDocumentsViewModel>(viewResult.Model);
        _mockContentService.Verify(repo => repo.SetDocumentToPublished(It.Is<CommonRequestBody>(x => x.Action == (int)ActionTypes.Unarchive), It.IsAny<AzureFunctionHeaderDetails>()));
    }

    [Fact]
    public async Task Check_error_view_when_news_article_is_not_archived()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = new CommonResponseBody() { Title = "testTitle", Body = "testBody", Id = "PlannedMaintenance" };
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;

        // Act
        var result = await controller.ArchiveNews(model).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.ArchiveError);
    }

    [Fact]
    public async Task Check_error_view_when_news_article_is_not_unarchived()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;

        // Act
        var result = await controller.UnarchiveNews(model).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UnarchiveError);
    }

    [Fact(Skip = "Proper fix follows & unblock Test")]
    public async Task check_news_article_is_saved_as_draft()
    {
        // Arrange
        CommonResponseBody commonResponseBody = new()
        {
            Title = "testTitle",
            Body = "testBody",
            Id = "NewsArticle",
            Published = false
        };
        ManageDocumentsViewModel model = new()
        {
            DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            //DocumentData = new  CommonResponseBody()
            //{
            //    Title = "testTitle",
            //    Body = "testBody",
            //    Id = "NewsArticle",
            //    Published = false
            //}
        };
        _ = _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        ManageDocumentsController controller = new ManageDocumentsController(
            _mockNewsService.Object,
            _mockDocRepo.Object,
            _mockContentService.Object,
            _mockGetNewsArticleByIdUseCase.Object,
            _mockGetNewsArticlesUseCase.Object);

        // Act
        string saveAsDraft = "SaveAsDraft";
        IActionResult result = await controller.SaveArticleAsDraft(model, saveAsDraft);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Check_news_article_is_deleted()
    {
        // Arrange
        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockNewsService.Setup(repo => repo.DeleteNewsArticle("1234")).ReturnsAsync(HttpStatusCode.OK);
        var controller = GetManageDocumentsController();

        // Act
        var result = await controller.DeleteNews(model);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void CreateNewsArticle_RendersView()
    {
        // Arrange
        ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.TempData = tempData;

        // Act
        IActionResult result = controller.CreateNewsArticle();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task EditNewsArticle_RendersView_With_Values()
    {
        // Arrange
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetailsWithSelectedNews();

        CommonResponseBody responseBody = new()
        {
            Id = "1",
            Title = "Some Test",
            Body = "Some body"
        };

        NewsArticle article = new()
        {
            Title = "Some Test",
            Body = "Some body",
            Id = "1",
            Pinned = true,
            DraftBody = string.Empty,
            DraftTitle = string.Empty
        };

        _mockGetNewsArticleByIdUseCase.Setup(useCase => useCase.HandleRequest(It.IsAny<GetNewsArticleByIdRequest>())).ReturnsAsync(new GetNewsArticleByIdResponse(article));

        ManageDocumentsController controller = GetManageDocumentsController();
        string editDocument = "EditDocument";

        // Act
        IActionResult result = await controller.EditNewsArticle(model, editDocument).ConfigureAwait(false);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        CommonResponseBodyViewModel documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).DocumentData;
        Assert.Equal(responseBody.Id, documentData.Id);
        Assert.Equal(responseBody.Title, documentData.Title);
        Assert.Equal(responseBody.Body, documentData.Body);
    }

    [Fact]
    public async Task PublishNewsArticle_Sets_ActionType_As_Published()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetailsWithSelectedNews();

        CommonResponseBody responseBody = new()
        {
            Id = "1",
            Title = "Some Test",
            Body = "Some body",
            Published = true
        };

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(responseBody);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = "Publish";

        // Act
        IActionResult result = await controller.PublishNewsArticle(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Check_error_view_when_PublishNewsArticle_fails_to_publish()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "Publish";

        // Act
        var result = await controller.PublishNewsArticle(model, publish);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.PublishError);
    }

    [Fact(Skip = "Proper fix follows & unblock Test")]
    public async Task PreviewNewsArticle_With_Values()
    {
        // Arrange
        var model = new ManageDocumentsViewModel
        {
            DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            DocumentData = new ViewModels.CommonResponseBodyViewModel()
            {
                Title = "testTitle",
                Body = "testBody",
                Id = "NewsArticle",
                Published = false
            },
            SelectedNewsId = "1"
        };

        var responseBody = new CommonResponseBody()
        {
            Id = "1",
            Title = "Some Test",
            Body = "Some body",
            Published = true
        };

        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(responseBody);

        ManageDocumentsController controller = GetManageDocumentsController();

        var create = "Create";
        var preview = "";
        // Act
        var result = await controller.PreviewNewsArticle(model, create, preview).ConfigureAwait(false);
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).DocumentData;
        Assert.Equal(responseBody.Id, documentData.Id);
        Assert.Equal(responseBody.Title, documentData.Title);
        Assert.Equal(responseBody.Body, documentData.Body);
    }

    [Fact]
    public async Task PreviewNewsArticle_ValidationChecks_When_No_Values_OnCreateNewsArticle()
    {
        // Arrange
        var model = new ManageDocumentsViewModel();

        var controller = GetManageDocumentsController();
        controller.ModelState.AddModelError("key", "title cannot be empty");
        var create = "Create";
        var preview = "";
        // Act
        var result = await controller.PreviewNewsArticle(model, create, preview).ConfigureAwait(false);
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/CreateNewsArticle", viewResult.ViewName);
    }

    [Fact]
    public async Task PreviewNewsArticle_ValidationChecks_When_No_Values_OnEditNewsArticle()
    {
        // Arrange
        var model = new ManageDocumentsViewModel();

        var controller = GetManageDocumentsController();
        controller.ModelState.AddModelError("key", "title cannot be empty");
        var create = "";
        var preview = "Preview";
        // Act
        var result = await controller.PreviewNewsArticle(model, create, preview).ConfigureAwait(false);
        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/EditNewsArticle", viewResult.ViewName);
    }

    [Fact(Skip = "Proper fix follows & unblock Test")]
    public async Task PreviewNewsArticle_When_Article_Is_Pinned()
    {
        // Arrange
        ManageDocumentsViewModel model = new()
        {
            DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            DocumentData = new CommonResponseBodyViewModel()
            {
                Title = "testTitle",
                Body = "testBody",
                Id = "NewsArticle",
                Published = false,
                Pinned = true
            },
            SelectedNewsId = "1"
        };

        CommonResponseBody responseBody = new()
        {
            Id = "1",
            Title = "Some Test",
            Body = "Some body",
            Published = false,
        };


        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(responseBody);
        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(responseBody);

        ManageDocumentsController controller = GetManageDocumentsController();

        string create = "Create";
        string preview = string.Empty;

        // Act
        IActionResult result = await controller.PreviewNewsArticle(model, create, preview).ConfigureAwait(false);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        CommonResponseBodyViewModel documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).DocumentData;
        Assert.Equal(responseBody.Id, documentData.Id);
        Assert.Equal(responseBody.Title, documentData.Title);
        Assert.Equal(responseBody.Body, documentData.Body);
        Assert.Equal(responseBody.Pinned, documentData.Pinned);
    }

    [Fact(Skip = "Proper fix follows & unblock Test")]
    public async Task PreviewNewsArticle_When_Article_Is_Not_Pinned()
    {
        // Arrange
        ManageDocumentsViewModel model = new()
        {
            DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            DocumentData = new CommonResponseBodyViewModel()
            {
                Title = "testTitle",
                Body = "testBody",
                Id = "NewsArticle",
                Published = false
            },
            SelectedNewsId = "1"
        };

        CommonResponseBody responseBody = new()
        {
            Id = "1",
            Title = "Some Test",
            Body = "Some body",
            Published = true
        };

        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(responseBody);

        ManageDocumentsController controller = GetManageDocumentsController();

        string create = "Create";
        string preview = "";

        // Act
        IActionResult result = await controller.PreviewNewsArticle(model, create, preview).ConfigureAwait(false);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        CommonResponseBodyViewModel documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).DocumentData;
        Assert.Equal(responseBody.Id, documentData.Id);
        Assert.Equal(responseBody.Title, documentData.Title);
        Assert.Equal(responseBody.Body, documentData.Body);
        Assert.False(responseBody.Pinned);
    }

    [Fact]
    public async Task Check_error_view_when_PreviewNewsArticle_fails_to_save_draft()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = user,
                Session = _mockSession.Object
            }
        };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(
            (repo) => repo.AddOrUpdateDocument(
                It.IsAny<CommonRequestBody>(),
                It.IsAny<AzureFunctionHeaderDetails>()))
            .ReturnsAsync((CommonResponseBody)default);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string create = "";
        string preview = "Preview";

        // Act
        IActionResult result = await controller.PreviewNewsArticle(model, create, preview);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(ArticleErrorMessages.SaveDraftError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PreviewNewsArticle_fails_to_set_pinned()
    {
        // Arrange
        ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = user,
                Session = _mockSession.Object
            }
        };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string create = "";
        string preview = "Preview";

        // Act
        IActionResult result = await controller.PreviewNewsArticle(model, create, preview).ConfigureAwait(false);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(ArticleErrorMessages.UpdatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PreviewChanges_fails_to_set_pinned()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var preview = "Preview";

        // Act
        var result = await controller.PreviewChanges(model, preview).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UpdatedError);
    }

    [Fact]
    public async Task Check_error_view_when_SaveArticleAsDraft_fails_to_save_draft()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.SaveArticleAsDraft(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.SaveDraftError);
    }

    [Fact]
    public async Task Check_error_view_when_SaveArticleAsDraft_fails_to_set_pinned()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.SaveArticleAsDraft(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UpdatedError);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_save_draft_with_id_and_selectedNewsID()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetailsWithSelectedNews();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UpdatedError);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_save_draft_with_id_and_no_selectedNewsID()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UpdatedError);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_save_draft_with_no_id_and_no_selectedNewsID()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetailsNoID();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.CreatedError);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_publish_with_id_and_selectedNewsID()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetailsWithSelectedNews();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UpdatedError);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_publish_with_id_and_no_selectedNewsID()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.UpdatedError);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_publish_with_no_id_and_no_selectedNewsID()
    {
        // Arrange
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        var context = new ControllerContext() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        var commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        var model = _manageDocumentsResultsFake.GetDocumentDetailsNoID();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        var controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        var publish = "";

        // Act
        var result = await controller.PublishChanges(model, publish).ConfigureAwait(false);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        var errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(errorModel.UserErrorMessage, ArticleErrorMessages.CreatedError);
    }

    public ManageDocumentsController GetManageDocumentsController()
    {
        return new ManageDocumentsController(_mockNewsService.Object, _mockDocRepo.Object, _mockContentService.Object, _mockGetNewsArticleByIdUseCase.Object, _mockGetNewsArticlesUseCase.Object);
    }
}
