using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.Application.TextSanitiser.Abstraction.Handler;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Core.NewsArticles.Application.Models;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.CreateNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.DeleteNewsArticle;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticleById;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.GetNewsArticles;
using DfE.GIAP.Core.NewsArticles.Application.UseCases.UpdateNewsArticle;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.News;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.Admin.ManageDocuments;
using DfE.GIAP.Web.Extensions;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Admin;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Admin;

[Trait("Category", "Manage Documents Controller Unit Tests")]
public class ManageDocumentsControllerTests : IClassFixture<UserClaimsPrincipalFake>, IClassFixture<ManageDocumentsResultsFake>
{
    private readonly Mock<IOptions<AzureAppSettings>> _mockAzureAppSettings = new();
    private readonly Mock<ISession> _mockSession = new();
    private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
    private readonly ManageDocumentsResultsFake _manageDocumentsResultsFake;
    private readonly Mock<IContentService> _mockContentService = new();
    private readonly Mock<INewsService> _mockNewsService = new();
    private readonly Mock<IUseCase<GetNewsArticleByIdRequest, GetNewsArticleByIdResponse>> _mockGetNewsArticleByIdUseCase = new();
    private readonly Mock<IUseCase<GetNewsArticlesRequest, GetNewsArticlesResponse>> _mockGetNewsArticlesUseCase = new();
    private readonly Mock<IUseCaseRequestOnly<DeleteNewsArticleRequest>> _mockDeleteNewsArticleUseCase = new();
    private readonly Mock<IUseCaseRequestOnly<CreateNewsArticleRequest>> _mockCreateNewsArticleUseCase = new();
    private readonly Mock<IUseCaseRequestOnly<UpdateNewsArticleRequest>> _mockUpdateNewsArticleUseCase = new();
    private readonly Mock<ITextSanitiserInvoker> _mockTextSanitiser = new();

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
        var expectedDocumentTypes = Enum
            .GetValues(typeof(DocumentType))
            .Cast<DocumentType>()
            .Select(dt => new
            {
                DocumentId = dt.ToString(),
                DocumentName = dt.GetDescription()
            })
            .OrderBy(x => x.DocumentName)
            .ToList();

        ManageDocumentsController controller = GetManageDocumentsController();

        // Act
        IActionResult result = await controller.ManageDocuments(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>());

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewData.ContainsKey("ListOfDocuments"));

        SelectList selectList = Assert.IsType<SelectList>(viewResult.ViewData["ListOfDocuments"]);

        List<SelectListItem> selectListItems = selectList.Cast<SelectListItem>().ToList();
        Assert.Equal(expectedDocumentTypes.Count, selectListItems.Count);

        for (int i = 0; i < expectedDocumentTypes.Count; i++)
        {
            Assert.Equal(expectedDocumentTypes[i].DocumentId, selectListItems[i].Value);
            Assert.Equal(expectedDocumentTypes[i].DocumentName, selectListItems[i].Text);
        }
    }

    [Fact]
    public async Task LoadNewsDocuments_When_ManageDocuments_Posted_MethodIsCalled_And_User_Has_Selected_News_article()
    {
        // Arrange
        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(commonResponseBody);

        ManageDocumentsViewModel model = new() { DocumentList = new Document { Id = 1, DocumentName = "Test title", DocumentId = "NewsArticle" } };

        ManageDocumentsController controller = GetManageDocumentsController();

        // Act
        IActionResult result = await controller.ManageDocuments(model, string.Empty, string.Empty);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(2, viewResult.ViewData.Values.Count);
        Assert.True(viewResult.ViewData.ContainsKey("ListOfDocuments"));
    }

    [Fact]
    public async Task SelectedDocument_FromDropDownList_PostBackToManageDocumentsMethod()
    {
        // Arrange
        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.GetContent(DocumentType.PlannedMaintenance)).ReturnsAsync(commonResponseBody);

        ManageDocumentsController controller = GetManageDocumentsController();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();
        string discard = "Discard";
        string editDocument = "EditDocument";

        // Act
        IActionResult result = await controller.ManageDocuments(model, discard, editDocument);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        CommonResponseBodyViewModel documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).DocumentData;
        Assert.Equal(commonResponseBody.Id, documentData.Id);
        Assert.Equal(2, viewResult.ViewData.Values.Count);
        Assert.True(viewResult.ViewData.ContainsKey("ListOfDocuments"));
    }

    [Fact]
    public async Task PublishChangesInDocument_PostBackMethod()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();

        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        List<Document> documentsList = new()
        {
            new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1, IsEnabled = true },
            new Document() { Id = 2, DocumentId = "PublicationSchedule", DocumentName = "Publication Schedule", SortId = 2, IsEnabled = true },
            new Document() { Id = 3, DocumentId = "PlannedMaintenance", DocumentName = "Planned Maintenance", SortId = 3, IsEnabled = true }
        };

        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;

        string publish = "Publish";
        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void CreateNewsArticle_RendersView()
    {
        // Arrange
        ITempDataProvider tempDataProvider = Mock.Of<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
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
        model.NewsArticle = new NewsArticleViewModel
        {
            Title = "Some Test",
            Body = "Some body",
            Id = "1",
            Pinned = true,
        };

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
            Id = NewsArticleIdentifier.From("1"),
            Pinned = true,
            DraftBody = string.Empty,
            DraftTitle = string.Empty
        };

        _mockGetNewsArticleByIdUseCase.Setup(useCase => useCase.HandleRequestAsync(It.IsAny<GetNewsArticleByIdRequest>())).ReturnsAsync(new GetNewsArticleByIdResponse(article));
        _mockTextSanitiser
            .Setup((t) => t.Handle(It.IsAny<string>()))
            .Returns((string input) => new SanitisedText(input));

        ManageDocumentsController controller = GetManageDocumentsController();

        // Act
        IActionResult result = await controller.UpdateNewsArticle(model);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        NewsArticleViewModel documentData = Assert.IsType<ManageDocumentsViewModel>(viewResult.ViewData.Model).NewsArticle;
        Assert.Equal(responseBody.Id, documentData.Id);
        Assert.Equal(responseBody.Title, documentData.Title);
        Assert.Equal(responseBody.Body, documentData.Body);
    }

    [Fact]
    public async Task Check_error_view_when_PreviewChanges_fails_to_set_pinned()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string preview = "Preview";

        // Act
        IActionResult result = await controller.PreviewChanges(model, preview);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.UpdatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_save_draft_with_id_and_selectedNewsID()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetailsWithSelectedNews();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = string.Empty;

        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.UpdatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_save_draft_with_id_and_no_selectedNewsID()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = string.Empty;

        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.UpdatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_save_draft_with_no_id_and_no_selectedNewsID()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetailsNoID();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = string.Empty;

        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.CreatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_publish_with_id_and_selectedNewsID()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetailsWithSelectedNews();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = string.Empty;

        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.UpdatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_publish_with_id_and_no_selectedNewsID()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetails();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = string.Empty;

        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.UpdatedError, errorModel.UserErrorMessage);
    }

    [Fact]
    public async Task Check_error_view_when_PublishChanges_fails_to_publish_with_no_id_and_no_selectedNewsID()
    {
        // Arrange
        System.Security.Claims.ClaimsPrincipal user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
        ControllerContext context = new() { HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession.Object } };

        CommonResponseBody commonResponseBody = _manageDocumentsResultsFake.GetCommonResponseBody();
        ManageDocumentsViewModel model = _manageDocumentsResultsFake.GetDocumentDetailsNoID();

        _mockContentService.Setup(repo => repo.SetDocumentToPublished(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync((CommonResponseBody)default);
        _mockContentService.Setup(repo => repo.AddOrUpdateDocument(It.IsAny<CommonRequestBody>(), It.IsAny<AzureFunctionHeaderDetails>())).ReturnsAsync(commonResponseBody);

        ManageDocumentsController controller = GetManageDocumentsController();
        controller.ControllerContext = context;
        string publish = string.Empty;

        // Act
        IActionResult result = await controller.PublishChanges(model, publish);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("../Admin/ManageDocuments/Error", viewResult.ViewName);
        UserErrorViewModel errorModel = Assert.IsType<UserErrorViewModel>(viewResult.Model);
        Assert.Equal(Messages.NewsArticle.Errors.CreatedError, errorModel.UserErrorMessage);
    }

    public ManageDocumentsController GetManageDocumentsController()
    {
        return new ManageDocumentsController(
            _mockNewsService.Object,
            _mockContentService.Object,
            _mockGetNewsArticleByIdUseCase.Object,
            _mockGetNewsArticlesUseCase.Object,
            _mockDeleteNewsArticleUseCase.Object,
            _mockCreateNewsArticleUseCase.Object,
            _mockUpdateNewsArticleUseCase.Object,
            _mockTextSanitiser.Object);
    }
}
