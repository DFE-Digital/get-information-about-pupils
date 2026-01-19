using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Search.FurtherEducation;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Controllers.LearnerNumber;
using DfE.GIAP.Web.Controllers.LearnerNumber.Mappers;
using DfE.GIAP.Web.Features.Auth.Application.Claims;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch.Mappers.TestDoubles;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;
using System.Security.Claims;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.Enums;

namespace DfE.GIAP.Web.Tests.Controllers.Search.LearnerNumber;

public class FELearnerNumberControllerTests : IClassFixture<PaginatedResultsFake>
{
    private readonly ILogger<FELearnerNumberController> _mockLogger = Substitute.For<ILogger<FELearnerNumberController>>();
    private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
    private readonly ISelectionManager _mockSelectionManager = Substitute.For<ISelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private readonly IUseCase<SearchRequest, SearchResponse> _mockUseCase =
        Substitute.For<IUseCase<SearchRequest, SearchResponse>>();
    private AzureAppSettings _mockAppSettings = new();
    private readonly IMapper<LearnerNumericSearchMappingContext, LearnerNumberSearchViewModel> _mockLearnerNumberSearchResponseToViewModelMapper =
        Substitute.For<IMapper<LearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>>();
    private readonly SessionFake _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;

    public FELearnerNumberControllerTests(PaginatedResultsFake paginatedResultsFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
        SearchResponse response =
            SearchByKeyWordsResponseTestDouble.CreateSuccessResponse();

        _mockUseCase.HandleRequestAsync(
            Arg.Any<SearchRequest>()).Returns(response);

        _mockLearnerNumberSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerNumericSearchMappingContext>()).Returns(
            new LearnerNumberSearchViewModel()
            {
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });
    }

    [Fact]
    public async Task PupilUlnSearch_returns_empty_page_when_first_navigated_to_FE_Estab_Type_User()
    {
        // arrange
        FELearnerNumberController sut = GetController();

        // act
        IActionResult result = await sut.PupilUlnSearch(null);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task PupilUlnSearch_returns_empty_page_when_first_navigated_to_Admin_User()
    {
        // arrange
        FELearnerNumberController sut = GetController();
        sut.ControllerContext.HttpContext.User = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();

        // act
        IActionResult result = await sut.PupilUlnSearch(null);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task PupilUlnSearch_returns_empty_page_when_first_navigated_to_User_With_Age_Access()
    {
        // arrange
        FELearnerNumberController sut = GetController();
        sut.ControllerContext.HttpContext.User = UserClaimsPrincipalFake.GetSpecificUserClaimsPrincipal(
             DsiKeys.OrganisationCategory.Establishment,
             DsiKeys.EstablishmentType.CommunitySchool, //not relevant for this test
             AuthRoles.Approver,
                18,
                25);

        // act
        IActionResult result = await sut.PupilUlnSearch(null);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task PupilUlnSearch_redirects_to_error_page_nonFE_User()
    {
        // arrange
        // act
        FELearnerNumberController sut = GetController();
        sut.ControllerContext.HttpContext.User = UserClaimsPrincipalFake.GetUserClaimsPrincipal();
        IActionResult result = await sut.PupilUlnSearch(null);

        // assert

        RedirectToActionResult? redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Error", redirectResult.ActionName);
        Assert.Equal("Home", redirectResult.ControllerName);
    }

    [Fact]
    public async Task PupilUlnSearch_returns_search_page_when_returned_to()
    {
        // arrange
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToHashSet());

        // act
        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        IActionResult result = await sut.PupilUlnSearch(true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(_paginatedResultsFake.GetUlns(), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task PupilUlnSearch_returns_a_page_of_results_when_searched()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        // act
        FELearnerNumberController sut = GetController();

        SetupSession();

        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task PupilUlnSearch_returns_another_page_of_results_when_navigated_to()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            PageLearnerNumbers = string.Join(',', _paginatedResultsFake.GetUlns().FormatLearnerNumbers())
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. ulns.FormatLearnerNumbers()]);

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(false);
    }

    [Fact]
    public async Task PupilUlnSearch_select_all_works()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        Assert.True(model.ToggleSelectAll);
    }

    [Fact]
    public async Task PupilUlnSearch_deselect_all_works()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        Assert.False(model.ToggleSelectAll);
    }

    [Fact]
    public async Task PupilUlnSearch_changes_selection_on_page_if_selections_are_different()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        SetupSession();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(["6424316654"]);

        FELearnerNumberController sut = GetController();

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        _mockSelectionManager.Received().AddAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "6424316654" })));
        _mockSelectionManager.Received().RemoveAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "7621706219" })));
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
    }

    [Fact]
    public async Task PupilUlnSearch_shows_error_if_no_ULNs_inputted()
    {
        // arrange
        LearnerNumberSearchViewModel inputModel = new() { LearnerNumberLabel = "ULN" };

        FELearnerNumberController sut = GetController();
        sut.ModelState.AddModelError("test", "<span style='display:none'>1</span>");

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(Messages.Search.Errors.EnterULNs, model.SearchBoxErrorMessage);
    }

    [Fact]
    public async Task PupilUlnSearch_shows_invalid_ULNs_on_search_if_they_exist()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlnsWithInvalid();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();
        // act

        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Single(model.Invalid);
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilUlnSearch_shows_not_found_UPNs_on_search_if_they_do_not_exist()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlnsWithNotFound();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = "A203202811068\r\n7621706219\r\n",
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. ulns.FormatLearnerNumbers()]);

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, "A203202811068\r\n7621706219\r\n");

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", false);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Single(model.NotFound);
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilUlnSearch_shows_duplicate_UPNs_on_search_if_they_exist()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlnsWithDuplicates();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        // act
        FELearnerNumberController sut = GetController();

        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Single(model.Duplicates);
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilUlnSearch_ensure_reset_on_search_works()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);
        SetupSession();

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionSortField, string.Empty);
        _mockSession.SetString(sut.SearchSessionSortDirection, string.Empty);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);

        // Ensure call to clear selection manager has been called on reset.
        _mockSelectionManager.Received().Clear();
        // Ensure all expected session keys have been removed on reset.
        Assert.False(_mockSession.Keys.Contains(sut.SearchSessionSortField),
            "The key 'SearchULN_SearchTextSortField' should have been removed from session");
        Assert.False(_mockSession.Keys.Contains(sut.SearchSessionSortDirection),
            "The key 'SearchULN_SearchTextSortDirection' should have been removed from session");
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilUlnSearch_ensure_Session_persisted_sorting_is_set_on_returned_view_model()
    {
        // arrange
        const string TestSortDirection = "ASC";
        const string TestSortField = "TEST_FIELD";

        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);
        SetupSession();


        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionSortField, TestSortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, TestSortDirection);

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);

        // Ensure all expected session keys have been removed on reset.
        Assert.True(_mockSession.Keys.Contains(sut.SearchSessionSortField),
            "The key 'SearchULN_SearchTextSortField' should be in session");
        Assert.True(_mockSession.Keys.Contains(sut.SearchSessionSortDirection),
            "The key 'SearchULN_SearchTextSortDirection' should be in session");

        // Ensure the session-based sorting values have been propogated to the model.
        Assert.Equal(TestSortField, model.SortField);
        Assert.Equal(TestSortDirection, model.SortDirection);

        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilUlnSearch_ensure_missing_learner_number_on_model_returns_to_search_with_no_learners()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            // Omit the 'LearnerNumber' from the view model.
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        FELearnerNumberController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);

        // Ensure the learner number is still not present and we get no learners by default.
        Assert.True(string.IsNullOrEmpty(model.LearnerNumber));
        Assert.Empty(model.Learners);
    }

    [Fact]
    public async Task PupilUlnSearch_search_works_with_empty_paginated_response()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        // Omit the Learners from the response, to ensure the
        // model.Total condition is exercised in the controller.
        PaginatedResponse paginatedResponse = new();
        paginatedResponse.ToggleSelectAll(false);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(0, model.Total);
    }

    [Fact]
    public async Task PupilUlnSearch_search_works_with_notPaged_true()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = "A203102209083\r\nA203202811068",
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. "A203102209083\r\nA203202811068".FormatLearnerNumbers()]);

        FELearnerNumberController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "", calledByController: false);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(2, model.Learners.ToList().Count);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_when_navigated_to()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            PageLearnerNumbers = string.Join(',', _paginatedResultsFake.GetUlns().FormatLearnerNumbers())
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(false);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_when_select_all_chosen()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        Assert.True(model.ToggleSelectAll);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_if_deselect_all_chosen()
    {
        // arrange
        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = ["6424316654"],
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        Assert.False(model.ToggleSelectAll);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_in_session_if_returnToSearch_true()
    {
        // arrange
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToHashSet());

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        string sortField = "Forename";
        string sortDirection = "asc";
        _mockSession.SetString(sut.SearchSessionSortField, sortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, sortDirection);

        // act
        IActionResult result = await sut.PupilUlnSearch(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(_paginatedResultsFake.GetUlns(), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_sets_download_link_if_returnToSearch_true()
    {
        // arrange
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToHashSet());

        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        IActionResult result = await sut.PupilUlnSearch(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedFurtherEducationLink, model.DownloadSelectedLink);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_forename_asc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_forename_desc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Forename";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_middlenames_asc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "MiddleNames";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_middlenames_desc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();
        string sortField = "MiddleNames";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_surname_asc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();
        string sortField = "Surname";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_surname_desc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Surname";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_gender_asc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Gender";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        
        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_gender_desc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Gender";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_dob_asc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Dob";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_dob_desc_correctly()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        FELearnerNumberController sut = GetController();

        string sortField = "Dob";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task ToDownloadSelectedULNData_returns_to_search_page_with_error_if_no_pupil_selected()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);
        SetupSession();
        FELearnerNumberController sut = GetController();

        // act
        IActionResult result = await sut.ToDownloadSelectedULNData(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task ToDownloadSelectedULNData_returns_options_page_when_pupils_selected()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet<string>());

        string joinedSelectedPupils = string.Join(',', ulns.FormatLearnerNumbers());

        // act
        FELearnerNumberController sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();
        IActionResult result = await sut.ToDownloadSelectedULNData(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);
        Assert.Equal(model.SelectedPupils, joinedSelectedPupils);
        Assert.True(model.SelectedPupilsCount == ulns.FormatLearnerNumbers().Length);
        Assert.Equal(model.LearnerNumber, ulns);
    }

    [Fact]
    public async Task ToDownloadSelectedULNData_returns_to_search_page_if_no_selected_pupil_in_model()
    {
        string ulns = _paginatedResultsFake.GetUlns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            // Omit the list of SelectedPupil's (i.e. pass null)
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);
        SetupSession();
        FELearnerNumberController sut = GetController();

        // act
        IActionResult result = await sut.ToDownloadSelectedULNData(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_redirects_to_uln_search_if_SelectedPupils_empty()
    {
        // arrange
        LearnerDownloadViewModel inputDownloadModel = new();

        FELearnerNumberController sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // act
        IActionResult result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(UniqueLearnerNumberLabels.SearchUlnActionName, redirectResult.ActionName);
        Assert.Equal(UniqueLearnerNumberLabels.SearchUlnControllerName, redirectResult.ControllerName);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_to_options_page_if_no_type_selected()
    {
        string[] ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', ulns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerNumberController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);
        Assert.Equal(model.SelectedPupils, joinedSelectedPupils);
        Assert.True(model.SelectedPupilsCount == ulns.Length);
        Assert.Equal(Messages.Search.Errors.SelectOneOrMoreDataTypes, sut.TempData["ErrorDetails"]);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_to_options_page_if_no_download_type_selected()
    {
        string[] ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', ulns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = [],
            DownloadFileType = DownloadFileType.None
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerNumberController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        
        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);
        Assert.Equal(model.SelectedPupils, joinedSelectedPupils);
        Assert.True(model.SelectedPupilsCount == ulns.Length);
        Assert.Equal(Messages.Search.Errors.SelectFileType, sut.TempData["ErrorDetails"]);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_to_options_page_if_no_download_data_exists()
    {
        string[] ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', ulns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = [],
            DownloadFileType = DownloadFileType.CSV
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerNumberController sut = GetController();
        sut.TempData = tempData;

        _mockDownloadService.GetFECSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>())
            .Returns(new ReturnFile());

        // act
        IActionResult result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);
        Assert.Equal(model.SelectedPupils, joinedSelectedPupils);
        Assert.True(model.SelectedPupilsCount == ulns.Length);
        Assert.Equal(Messages.Downloads.Errors.NoDataForSelectedPupils, sut.TempData["ErrorDetails"]);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_redirects_to_error_page_if_download_null()
    {
        string[] ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', ulns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = [],
            DownloadFileType = DownloadFileType.CSV
        };

        FELearnerNumberController sut = GetController();

        // act
        IActionResult result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.Equal(Routes.Application.Error, redirectResult.ActionName);
        Assert.Equal(Routes.Application.Home, redirectResult.ControllerName);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_data()
    {
        string[] ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', ulns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = [],
            DownloadFileType = DownloadFileType.CSV
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerNumberController sut = GetController();
        sut.TempData = tempData;

        _mockDownloadService.GetFECSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>())
            .Returns(new ReturnFile()
            {
                FileName = "test",
                FileType = FileType.ZipFile,
                Bytes = []
            });

        // act
        IActionResult result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    private static void AssertAbstractValues(FELearnerNumberController controller, LearnerNumberSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.SearchAction, model.SearchAction);
        Assert.Equal(Global.FELearnerTextSearchController, model.FullTextLearnerSearchController);
        Assert.Equal(Global.FELearnerTextSearchAction, model.FullTextLearnerSearchAction);
        Assert.False(model.ShowLocalAuthority);
    }

    private FELearnerNumberController GetController()
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetFEApproverClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumULNsPerSearch = 4000,
            CommonTransferFileUPNLimit = 4000,
            DownloadOptionsCheckLimit = 500
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        ControllerContext context = new()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession }
        };
        context.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        Mock<IEventLogger> mockEventLogger = new();

        List<AvailableDatasetResult> availableDatasetResults =
            [
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.PP, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.SEN, HasData: true, CanDownload: true)
            ];
        GetAvailableDatasetsForPupilsResponse response = new(availableDatasetResults);

        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(getAvailableDatasetsResponse);

        DownloadPupilDataResponse downloadPupilDataResponse = new();
        Mock<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>> mockDownloadPupilDataUseCase = new();
        mockDownloadPupilDataUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<DownloadPupilDataRequest>()))
            .ReturnsAsync(downloadPupilDataResponse);

        IReadOnlyList<string> validSortFields = new List<string> { "MockSortField" };

        Mock<IMapper<(string SortField, string SortDirection), SortOrder>> mockMapper = new();

        mockMapper
            .Setup((mapper) => mapper.Map(It.IsAny<(string, string)>()))
            .Returns(new SortOrder(validSortFields[0], "asc", validSortFields));

        return new FELearnerNumberController(
            _mockUseCase,
            _mockLearnerNumberSearchResponseToViewModelMapper,
            mockMapper.Object,
            _mockLogger,
            _mockDownloadService,
            _mockSelectionManager,
            _mockAppOptions,
            mockEventLogger.Object,
            mockGetAvailableDatasetsForPupilsUseCase.Object,
            mockDownloadPupilDataUseCase.Object)
        {
            ControllerContext = context
        };
    }

    private void SetupSession()
    {
        _mockSession.SetString("SearchULN_SearchText", _paginatedResultsFake.GetUlns());
        _mockSession.SetString(
          _paginatedResultsFake.TotalSearchResultsSessionKey,
          _paginatedResultsFake.TotalSearchResultsSessionValue);
    }
}
