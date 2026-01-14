using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Search.FurtherEducation;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Models.Common;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Search;
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
    private readonly TestSession _mockSession = new();
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

    #region Search

    [Fact]
    public async Task PupilUlnSearch_returns_empty_page_when_first_navigated_to_FE_Estab_Type_User()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        // act
        var sut = GetController();
        var result = await sut.PupilUlnSearch(null);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task PupilUlnSearch_returns_empty_page_when_first_navigated_to_Admin_User()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        // act
        var sut = GetController();
        sut.ControllerContext.HttpContext.User = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        var result = await sut.PupilUlnSearch(null);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task PupilUlnSearch_returns_empty_page_when_first_navigated_to_User_With_Age_Access()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        // act
        var sut = GetController();
        sut.ControllerContext.HttpContext.User = new UserClaimsPrincipalFake().GetSpecificUserClaimsPrincipal(
             DsiKeys.OrganisationCategory.Establishment,
             DsiKeys.EstablishmentType.CommunitySchool, //not relevant for this test
             AuthRoles.Approver,
                18,
                25);
        var result = await sut.PupilUlnSearch(null);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task PupilUlnSearch_redirects_to_error_page_nonFE_User()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        // act
        var sut = GetController();
        sut.ControllerContext.HttpContext.User = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();
        var result = await sut.PupilUlnSearch(null);

        // assert

        Assert.IsType<RedirectToActionResult>(result);
        var redirectResult = result as RedirectToActionResult;
        Assert.True(redirectResult.ActionName.Equals("Error"));
        Assert.True(redirectResult.ControllerName.Equals("Home"));
    }

    [Fact]
    public async Task PupilUlnSearch_returns_search_page_when_returned_to()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var result = await sut.PupilUlnSearch(true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(_paginatedResultsFake.GetUlns(), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task PupilUlnSearch_returns_a_page_of_results_when_searched()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        SetupSession();

        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task PupilUlnSearch_returns_another_page_of_results_when_navigated_to()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string ulns = _paginatedResultsFake.GetUlns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = ulns,
            PageLearnerNumbers = string.Join(',', _paginatedResultsFake.GetUlns().FormatLearnerNumbers())
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. ulns.FormatLearnerNumbers()]);

        // act
        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        IActionResult result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(false);
    }

    [Fact]
    public async Task PupilUlnSearch_select_all_works()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };


        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = new List<string>() { "6424316654" },
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        var paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };


        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = new List<string>() { "6424316654" },
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        var paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = new List<string>() { "6424316654" },
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        SetupSession();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>() { "6424316654" });

        // act
        var sut = GetController();

        var result = await sut.PupilUlnSearch(inputModel, 1, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var inputModel = new LearnerNumberSearchViewModel() { LearnerNumberLabel = "ULN" };

        // act
        var sut = GetController();
        sut.ModelState.AddModelError("test", "<span style='display:none'>1</span>");

        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(Messages.Search.Errors.EnterULNs, model.SearchBoxErrorMessage);
    }

    [Fact]
    public async Task PupilUlnSearch_shows_invalid_ULNs_on_search_if_they_exist()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlnsWithInvalid();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        // act
        var sut = GetController();

        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(model.Invalid.Count == 1);
        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilUlnSearch_shows_not_found_UPNs_on_search_if_they_do_not_exist()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string ulns = _paginatedResultsFake.GetUlnsWithNotFound();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = "A203202811068\r\n7621706219\r\n",
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. ulns.FormatLearnerNumbers()]);

        // act
        FELearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, "A203202811068\r\n7621706219\r\n");

        IActionResult result = await sut.PupilUlnSearch(inputModel, 0, "", "", false);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(model.NotFound.Count == 1);
        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilUlnSearch_shows_duplicate_UPNs_on_search_if_they_exist()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlnsWithDuplicates();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        // act
        var sut = GetController();

        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(model.Duplicates.Count == 1);
        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilUlnSearch_ensure_reset_on_search_works()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        SetupSession();

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionSortField, string.Empty);
        _mockSession.SetString(sut.SearchSessionSortDirection, string.Empty);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);

        // Ensure call to clear selection manager has been called on reset.
        _mockSelectionManager.Received().Clear();
        // Ensure all expected session keys have been removed on reset.
        Assert.False(_mockSession.Keys.Contains(sut.SearchSessionSortField),
            "The key 'SearchULN_SearchTextSortField' should have been removed from session");
        Assert.False(_mockSession.Keys.Contains(sut.SearchSessionSortDirection),
            "The key 'SearchULN_SearchTextSortDirection' should have been removed from session");
        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilUlnSearch_ensure_Session_persisted_sorting_is_set_on_returned_view_model()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        const string TestSortDirection = "ASC";
        const string TestSortField = "TEST_FIELD";

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        SetupSession();

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionSortField, TestSortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, TestSortDirection);

        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);

        // Ensure all expected session keys have been removed on reset.
        Assert.True(_mockSession.Keys.Contains(sut.SearchSessionSortField),
            "The key 'SearchULN_SearchTextSortField' should be in session");
        Assert.True(_mockSession.Keys.Contains(sut.SearchSessionSortDirection),
            "The key 'SearchULN_SearchTextSortDirection' should be in session");

        // Ensure the session-based sorting values have been propogated to the model.
        Assert.Equal(TestSortField, model.SortField);
        Assert.Equal(TestSortDirection, model.SortDirection);

        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilUlnSearch_ensure_missing_learner_number_on_model_returns_to_search_with_no_learners()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            // Omit the 'LearnerNumber' from the view model.
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);

        // Ensure the learner number is still not present and we get no learners by default.
        Assert.True(string.IsNullOrEmpty(model.LearnerNumber));
        Assert.True(model.Learners.Count() == 0);
    }

    [Fact]
    public async Task PupilUlnSearch_search_works_with_empty_paginated_response()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = new List<string>() { "6424316654" },
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        // Omit the Learners from the response, to ensure the
        // model.Total condition is exercised in the controller.
        var paginatedResponse = new PaginatedResponse();
        paginatedResponse.ToggleSelectAll(false);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var result = await sut.PupilUlnSearch(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(0, model.Total);
    }

    [Fact]
    public async Task PupilUlnSearch_search_works_with_notPaged_true()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

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
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(2, model.Learners.ToList().Count);
    }

    #endregion Search

    #region Sorting

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_when_navigated_to()
    {
        // arrange
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            PageLearnerNumbers = String.Join(',', _paginatedResultsFake.GetUlns().FormatLearnerNumbers())
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var sortField = "Forename";
        var sortDirection = "asc";
        var result = await sut.PupilUlnSearch(inputModel, 1, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = new List<string>() { "6424316654" },
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        var paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var sortField = "Forename";
        var sortDirection = "asc";
        var result = await sut.PupilUlnSearch(inputModel, 1, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();

        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = new List<string>() { "6424316654" },
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        var paginatedResponse = _paginatedResultsFake.GetValidULNLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString("missingLearnerNumbers", JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var sortField = "Forename";
        var sortDirection = "asc";
        var result = await sut.PupilUlnSearch(inputModel, 1, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var sortField = "Forename";
        var sortDirection = "asc";
        _mockSession.SetString(sut.SearchSessionSortField, sortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, sortDirection);
        var result = await sut.PupilUlnSearch(true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToHashSet());

        // act
        var sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUlns());

        var result = await sut.PupilUlnSearch(true);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedFurtherEducationLink, model.DownloadSelectedLink);
    }

    [Fact]
    public async Task PupilUlnSearch_updates_model_with_sorting_forename_asc_correctly()
    {
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "Forename";
        var sortDirection = "asc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "Forename";
        var sortDirection = "desc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "MiddleNames";
        var sortDirection = "asc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();
        var sortField = "MiddleNames";
        var sortDirection = "desc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();
        var sortField = "Surname";
        var sortDirection = "asc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "Surname";
        var sortDirection = "desc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "Gender";
        var sortDirection = "asc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "Gender";
        var sortDirection = "desc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());

        SetupSession();

        var sut = GetController();

        var sortField = "Dob";
        var sortDirection = "asc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

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
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = _paginatedResultsFake.GetUlns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet());
        SetupSession();

        var sut = GetController();

        var sortField = "Dob";
        var sortDirection = "desc";

        // act
        var result = await sut.PupilUlnSearch(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUlns()), model.LearnerNumber);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUlns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    #endregion Sorting

    #region Download CSV

    [Fact]
    public async Task ToDownloadSelectedULNData_returns_to_search_page_with_error_if_no_pupil_selected()
    {
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        SetupSession();
        var sut = GetController();

        // act
        var result = await sut.ToDownloadSelectedULNData(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task ToDownloadSelectedULNData_returns_options_page_when_pupils_selected()
    {
        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            SelectedPupil = ulns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(ulns.FormatLearnerNumbers().ToHashSet<string>());

        var joinedSelectedPupils = String.Join(',', ulns.FormatLearnerNumbers());

        // act
        var sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();
        var result = await sut.ToDownloadSelectedULNData(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerDownloadViewModel;

        Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == ulns.FormatLearnerNumbers().Length);
        Assert.True(model.LearnerNumber.Equals(ulns));
    }

    [Fact]
    public async Task ToDownloadSelectedULNData_returns_to_search_page_if_no_selected_pupil_in_model()
    {
        var newsPubCommonResponse = new CommonResponseBody()
        {
            Id = "0",
            Body = "test"
        };

        var ulns = _paginatedResultsFake.GetUlns();
        var inputModel = new LearnerNumberSearchViewModel()
        {
            LearnerNumber = ulns,
            // Omit the list of SelectedPupil's (i.e. pass null)
            PageLearnerNumbers = string.Join(',', ulns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        SetupSession();
        var sut = GetController();

        // act
        var result = await sut.ToDownloadSelectedULNData(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(viewResult.ViewName.Equals(Global.SearchView));

        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_redirects_to_uln_search_if_SelectedPupils_empty()
    {
        // arrange
        var inputDownloadModel = new LearnerDownloadViewModel();

        var sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // act
        var result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
        var redirectResult = result as RedirectToActionResult;
        Assert.Equal(UniqueLearnerNumberLabels.SearchUlnActionName, redirectResult.ActionName);
        Assert.Equal(UniqueLearnerNumberLabels.SearchUlnControllerName, redirectResult.ControllerName);
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_to_options_page_if_no_type_selected()
    {
        var ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        var joinedSelectedPupils = String.Join(',', ulns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

        // act
        var result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerDownloadViewModel;

        Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == ulns.Length);
        Assert.True(sut.TempData["ErrorDetails"].Equals(Messages.Search.Errors.SelectOneOrMoreDataTypes));
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_to_options_page_if_no_download_type_selected()
    {
        var ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        var joinedSelectedPupils = String.Join(',', ulns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.None
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

        // act
        var result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerDownloadViewModel;

        Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == ulns.Length);
        Assert.True(sut.TempData["ErrorDetails"].Equals(Messages.Search.Errors.SelectFileType));
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_to_options_page_if_no_download_data_exists()
    {
        var ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        var joinedSelectedPupils = String.Join(',', ulns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.CSV
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

        _mockDownloadService.GetFECSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>())
            .Returns(new ReturnFile());

        // act
        var result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerDownloadViewModel;

        Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == ulns.Length);
        Assert.True(sut.TempData["ErrorDetails"].Equals(Messages.Downloads.Errors.NoDataForSelectedPupils));
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_redirects_to_error_page_if_download_null()
    {
        var ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        var joinedSelectedPupils = String.Join(',', ulns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.CSV
        };

        var sut = GetController();

        // act
        var result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
        var redirectResult = result as RedirectToActionResult;

        Assert.True(redirectResult.ActionName.Equals(Routes.Application.Error));
        Assert.True(redirectResult.ControllerName.Equals(Routes.Application.Home));
    }

    [Fact]
    public async Task DownloadSelectedUlnDatabaseData_returns_data()
    {
        var ulns = _paginatedResultsFake.GetUlns().FormatLearnerNumbers();
        var joinedSelectedPupils = String.Join(',', ulns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = ulns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.CSV
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
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
                Bytes = new byte[0]
            });

        // act
        var result = await sut.DownloadSelectedUlnDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    #endregion Download CSV

    private void AssertAbstractValues(FELearnerNumberController controller, LearnerNumberSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.SearchAction, model.SearchAction);
        Assert.Equal(controller.FullTextLearnerSearchController, model.FullTextLearnerSearchController);
        Assert.Equal(controller.FullTextLearnerSearchAction, model.FullTextLearnerSearchAction);
        Assert.Equal(controller.ShowLocalAuthority, model.ShowLocalAuthority);
    }

    private FELearnerNumberController GetController()
    {
        var user = new UserClaimsPrincipalFake().GetFEApproverClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumULNsPerSearch = 4000,
            CommonTransferFileUPNLimit = 4000,
            DownloadOptionsCheckLimit = 500
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        var context = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession }
        };
        context.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        Mock<IEventLogger> mockEventLogger = new();

        List<AvailableDatasetResult> availableDatasetResults = new()
            {
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.PP, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.SEN, HasData: true, CanDownload: true)
            };
        GetAvailableDatasetsForPupilsResponse response = new(availableDatasetResults);

        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(response);

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
            mockGetAvailableDatasetsForPupilsUseCase.Object)
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
