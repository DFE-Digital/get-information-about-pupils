using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Models.Common;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Controllers.LearnerNumber;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.LearnerNumber;

public class PupilPremiumLearnerNumberControllerTests : IClassFixture<PaginatedResultsFake>
{
    private readonly ILogger<PupilPremiumLearnerNumberController> _mockLogger = Substitute.For<ILogger<PupilPremiumLearnerNumberController>>();
    private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
    private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
    private readonly ISelectionManager _mockSelectionManager = Substitute.For<ISelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsUseCaseMock = Substitute.For<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>();
    private AzureAppSettings _mockAppSettings = new();
    private readonly TestSession _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;

    public PupilPremiumLearnerNumberControllerTests(PaginatedResultsFake paginatedResultsFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
    }

    #region Search

    [Fact]
    public async Task PupilPremium_returns_empty_page_when_first_navigated_to()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        // act
        PupilPremiumLearnerNumberController sut = GetController();
        IActionResult result = await sut.PupilPremium(null);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);

    }

    [Fact]
    public async Task PupilPremium_returns_search_page_when_returned_to()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.PupilPremium(true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, _paginatedResultsFake.GetUpns());
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task PupilPremium_returns_a_page_of_results_when_searched()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task PupilPremium_returns_another_page_of_results_when_navigated_to()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            PageLearnerNumbers = String.Join(',', _paginatedResultsFake.GetUpns().FormatLearnerNumbers())
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(true);
    }

    [Fact]
    public async Task PupilPremium_select_all_works()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = new List<string>() { "A203102209083" },
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, paginatedResponse);

        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(true);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        Assert.True(model.ToggleSelectAll);
    }

    [Fact]
    public async Task PupilPremium_deselect_all_works()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = new List<string>() { "A203102209083" }
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, paginatedResponse);

        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "");

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        Assert.False(model.ToggleSelectAll);
    }

    [Fact]
    public async Task PupilPremium_changes_selection_on_page_if_selections_are_different()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = new List<string>() { "A203102209083" },
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>() { "A203102209083" });

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, paginatedResponse);

        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        _mockSelectionManager.Received().AddAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203102209083" })));
        _mockSelectionManager.Received().RemoveAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203202811068" })));
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
    }

    [Fact]
    public async Task PupilPremium_shows_error_if_no_UPNs_inputted()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new();

        // act
        PupilPremiumLearnerNumberController sut = GetController();
        sut.ModelState.AddModelError("test", "<span style='display:none'>1</span>");

        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(Messages.Search.Errors.EnterUPNs, model.SearchBoxErrorMessage);
    }

    [Fact]
    public async Task PupilPremium_shows_invalid_UPNs_on_search_if_they_exist()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = upns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        SetupPaginatedSearch(sut.IndexType, _paginatedResultsFake.GetInvalidLearners());

        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(model.Invalid.Count == 1);
        Assert.True(model.Learners.Count() == 3);
    }

    [Fact]
    public async Task PupilPremium_shows_not_found_UPNs_on_search_if_they_do_not_exist()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpnsWithNotFound();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = upns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", false);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(model.NotFound.Count == 1);
        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilPremium_shows_duplicate_UPNs_on_search_if_they_exist()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpnsWithDuplicates();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = upns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.True(model.Duplicates.Count == 1);
        Assert.True(model.Learners.Count() == 2);
    }

    [Fact]
    public async Task PupilPremium_populates_LearnerNumberIds_with_Id_when_UPN_0()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        PaginatedResponse response = new()
        {
            Learners = new List<Learner>()
                    {
                        new()
                        {
                            Id = "123",
                            LearnerNumber = "0",
                        },
                        new()
                        {
                            Id = "456",
                            LearnerNumber = "A203202811068",
                        }
                    },
            Count = 2
        };
        List<Learner> expectedLearners = new()
                    {
                        new Learner()
                        {
                           Id = "123",
                            LearnerNumber = "0",
                            LearnerNumberId = "123",
                        },
                        new Learner()
                        {
                            Id = "456",
                            LearnerNumber = "A203202811068",
                            LearnerNumberId = "A203202811068",
                        }
                    };
        SetupPaginatedSearch(sut.IndexType, response);

        // act

        IActionResult result = await sut.PupilPremium(true);

        // assert

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        Assert.Equal("123\nA203202811068", model.LearnerNumberIds);
        Assert.True(model.Learners.SequenceEqual(expectedLearners));
    }

    #endregion Search

    #region Sorting

    [Fact]
    public async Task PupilPremium_preserves_sort_settings_when_navigated_to()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            PageLearnerNumbers = String.Join(',', _paginatedResultsFake.GetUpns().FormatLearnerNumbers())
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Forename";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(true);
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_when_select_all_chosen()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = new List<string>() { "A203102209083" },
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, paginatedResponse);

        string sortField = "Forename";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(true);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        Assert.True(model.ToggleSelectAll);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_if_deselect_all_chosen()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = new List<string>() { "A203102209083" }
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, paginatedResponse);

        string sortField = "Forename";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        Assert.False(model.ToggleSelectAll);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_preserves_sort_settings_in_session_if_returnToSearch_true()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Forename";
        string sortDirection = "asc";
        _mockSession.SetString(sut.SearchSessionSortField, sortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, sortDirection);
        IActionResult result = await sut.PupilPremium(true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, _paginatedResultsFake.GetUpns());
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_forename_asc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Forename";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_forename_desc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Forename";
        string sortDirection = "desc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_middlenames_asc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "MiddleNames";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_middlenames_desc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "MiddleNames";
        string sortDirection = "desc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_surname_asc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Surname";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_surname_desc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Surname";
        string sortDirection = "desc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_gender_asc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Gender";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_gender_desc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Gender";
        string sortDirection = "desc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_dob_asc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Dob";
        string sortDirection = "asc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_dob_desc_correctly()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        string sortField = "Dob";
        string sortDirection = "desc";
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    #endregion Sorting

    #region Invalid UPNs

    [Fact]
    public async Task PPInvalidUpns_shows_invalid_upn_page_upns_only()
    {
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        InvalidLearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        IActionResult result = await sut.PPInvalidUPNs(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        InvalidLearnerNumberSearchViewModel? model = viewResult.Model as InvalidLearnerNumberSearchViewModel;

        Assert.True(model.Learners.Count() == 3);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
    }

    [Fact]
    public async Task PPInvalidUpns_shows_invalid_upn_page_ids_and_upns()
    {
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        InvalidLearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns
        };
        IEnumerable<Learner> expectedLearners = _paginatedResultsFake.GetInvalidLearners().Learners.Concat(_paginatedResultsFake.GetValidLearners().Learners);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.PPInvalidUPNs(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        InvalidLearnerNumberSearchViewModel? model = viewResult.Model as InvalidLearnerNumberSearchViewModel;

        Assert.True(model.Learners.Count() == 5);
        Assert.True(model.Learners.SequenceEqual(expectedLearners));
    }

    [Fact]
    public async Task PPInvalidUpns_shows_invalid_upn_page_ids_only()
    {
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        InvalidLearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, new PaginatedResponse());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetInvalidLearners());

        IActionResult result = await sut.PPInvalidUPNs(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        InvalidLearnerNumberSearchViewModel? model = viewResult.Model as InvalidLearnerNumberSearchViewModel;

        Assert.True(model.Learners.Count() == 3);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
    }

    [Fact]
    public async Task PPInvalidUpnsConfirmation_goes_to_MPL_if_asked()
    {
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        InvalidLearnerNumberSearchViewModel inputModel = new()
        {
            SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_MyPupilList
        };

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        IActionResult result = await sut.PPInvalidUPNsConfirmation(inputModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
        RedirectToActionResult? redirectResult = result as RedirectToActionResult;

        Assert.Equal(Global.MyPupilListAction, redirectResult.ActionName);
        Assert.Equal(Global.MyPupilListControllerName, redirectResult.ControllerName);
    }

    [Fact]
    public async Task PPInvalidUpnsConfirmation_goes_back_to_search_if_asked()
    {
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        InvalidLearnerNumberSearchViewModel inputModel = new()
        {
            SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_ReturnToSearch
        };

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        IActionResult result = await sut.PPInvalidUPNsConfirmation(inputModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
        RedirectToActionResult? redirectResult = result as RedirectToActionResult;

        Assert.Equal(redirectResult.ActionName, sut.SearchAction);
    }

    [Fact]
    public async Task PPInvalidUpnsConfirmation_shows_error_if_no_selection_made()
    {
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        InvalidLearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockPaginatedService.GetPage(
            Arg.Any<string>(),
            Arg.Any<Dictionary<string, string[]>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Is(sut.IndexType),
            Arg.Is(AzureSearchQueryType.Numbers),
            Arg.Any<AzureFunctionHeaderDetails>())
            .Returns(_paginatedResultsFake.GetInvalidLearners());
        _mockPaginatedService.GetPage(
          Arg.Any<string>(),
          Arg.Any<Dictionary<string, string[]>>(),
          Arg.Any<int>(),
          Arg.Any<int>(),
          Arg.Is(sut.IndexType),
          Arg.Is(AzureSearchQueryType.Id),
          Arg.Any<AzureFunctionHeaderDetails>())
          .Returns(new PaginatedResponse());

        IActionResult result = await sut.PPInvalidUPNsConfirmation(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        Assert.False(sut.ModelState.IsValid);
    }

    #endregion Invalid UPNs

    [Fact]
    public async Task AddToMyPupilList_adds_to_mpl()
    {
        // Arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // Act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            _paginatedResultsFake.GetUpns());

        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.PPAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.ItemAddedToMyPupilList);
    }

    [Fact]
    public async Task AddToMyPupilList_returns_search_page_with_error_if_no_pupil_selected()
    {
        // Arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager
            .GetSelected(Arg.Any<string[]>())
            .Returns([]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            _paginatedResultsFake.GetUpns());

        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task AddToMyPupilList_redirects_to_InvalidUPNs_if_they_exist()
    {
        // Arrange
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerNumberSearchViewModel inputModel = new ()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager
            .GetSelected(Arg.Any<string[]>())
            .Returns(upns.FormatLearnerNumbers().ToHashSet());

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            _paginatedResultsFake.GetUpns());

        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        InvalidLearnerNumberSearchViewModel model = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
    }

    [Fact]
    public async Task AddToMyPupilList_returns_an_error_if_over_limit()
    {
        // Arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        _addPupilsUseCaseMock.When(t => t.HandleRequestAsync(Arg.Any<AddPupilsToMyPupilsRequest>())).Throws(new MyPupilsLimitExceededException(1));

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            _paginatedResultsFake.GetUpns());

        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        Assert.Equal(Messages.Common.Errors.MyPupilListLimitExceeded, model.ErrorDetails);
    }

    #region Download

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_returns_data()
    {
        // arrange

        _mockDownloadService.GetPupilPremiumCSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>(),
            Arg.Any<UserOrganisation>()
            ).Returns(new ReturnFile()
            {
                FileName = "test",
                FileType = FileType.ZipFile,
                Bytes = new byte[0]
            });

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_redirects_to_error_page_if_download_null()
    {
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet<string>());

        // act
        PupilPremiumLearnerNumberController sut = GetController();

        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
        RedirectToActionResult? redirectResult = result as RedirectToActionResult;

        Assert.Equal(Routes.Application.Error, redirectResult.ActionName);
        Assert.Equal(Routes.Application.Home, redirectResult.ControllerName);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_with_empty_document_returns_to_search_page_with_errorDetails()
    {
        // arrange
        CommonResponseBody newsPubCommonResponse = new()
        {
            Id = "0",
            Body = "test"
        };

        _mockDownloadService.GetPupilPremiumCSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>(),
            Arg.Any<UserOrganisation>()
            ).Returns(new ReturnFile()
            {
                FileName = "test",
                FileType = FileType.ZipFile,
                // Omit the byte array to force the error!
            });

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
        };

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());
        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString("totalSearch", "20");
        SetupPaginatedSearchGetValidLearners(sut.IndexType);

        // act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        LearnerNumberSearchViewModel? model = viewResult.Model as LearnerNumberSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        Assert.Contains(Messages.Downloads.Errors.NoDataForSelectedPupils, model.ErrorDetails);
    }

    #endregion Download

    private void AssertAbstractValues(PupilPremiumLearnerNumberController controller, LearnerNumberSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
        Assert.Equal(controller.SearchAction, model.SearchAction);
        Assert.Equal(controller.FullTextLearnerSearchController, model.FullTextLearnerSearchController);
        Assert.Equal(controller.FullTextLearnerSearchAction, model.FullTextLearnerSearchAction);
    }

    private PupilPremiumLearnerNumberController GetController()
    {
        var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumUPNsPerSearch = 4000,
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);
        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));

        return new PupilPremiumLearnerNumberController(
            _mockLogger,
            _mockDownloadService,
            _mockPaginatedService,
            _mockSelectionManager,
            _mockAppOptions,
            _addPupilsUseCaseMock)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession }
            }
        };
    }

    private void SetupPaginatedSearch(AzureSearchIndexType indexType, AzureSearchQueryType azureSearchQueryType, PaginatedResponse paginatedResponse)
    {
        _mockPaginatedService.GetPage(
               Arg.Any<string>(),
               Arg.Any<Dictionary<string, string[]>>(),
               Arg.Any<int>(),
               Arg.Any<int>(),
               Arg.Is(indexType),
               Arg.Is(azureSearchQueryType),
               Arg.Any<AzureFunctionHeaderDetails>(),
               Arg.Any<string>(),
               Arg.Any<string>())
               .Returns(paginatedResponse);
    }

    private void SetupPaginatedSearch(AzureSearchIndexType indexType, Domain.Search.Learner.PaginatedResponse paginatedResponse)
    {
        _mockPaginatedService.GetPage(
            Arg.Any<string>(),
            Arg.Any<Dictionary<string, string[]>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Is(indexType),
            Arg.Is<AzureSearchQueryType>(x => x == AzureSearchQueryType.Numbers || x == AzureSearchQueryType.Id),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(paginatedResponse);
    }

    private void SetupPaginatedSearchGetValidLearners(AzureSearchIndexType indexType)
    {
        _mockPaginatedService.GetPage(
           Arg.Any<string>(),
            Arg.Any<Dictionary<string, string[]>>(),
            Arg.Any<int>(),
            Arg.Any<int>(),
            Arg.Is(indexType),
            Arg.Is<AzureSearchQueryType>(x => x == AzureSearchQueryType.Numbers || x == AzureSearchQueryType.Id),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<string>(),
            Arg.Any<string>())
            .Returns(_paginatedResultsFake.GetValidLearners());
    }
}
