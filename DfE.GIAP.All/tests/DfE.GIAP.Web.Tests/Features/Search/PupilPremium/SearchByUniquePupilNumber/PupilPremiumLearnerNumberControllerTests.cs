using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Tests.Features.Search.PupilPremium.TestDoubles;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.SearchByUniquePupilNumber;

public class PupilPremiumLearnerNumberControllerTests : IClassFixture<PaginatedResultsFake>
{
    private readonly ILogger<PupilPremiumLearnerNumberController> _mockLogger = Substitute.For<ILogger<PupilPremiumLearnerNumberController>>();
    private readonly ISelectionManager _mockSelectionManager = Substitute.For<ISelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsUseCaseMock = Substitute.For<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>();
    private readonly Mock<IUseCase<PupilPremiumSearchRequest, PupilPremiumSearchResponse>> _mockUseCase = new();
    private AzureAppSettings _mockAppSettings = new();
    private readonly SessionFake _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;

    private readonly Mock<
        IMapper<
            PupilPremiumLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>> _mockLearnerNumberSearchResponseToViewModelMapper = new();

    public PupilPremiumLearnerNumberControllerTests(PaginatedResultsFake paginatedResultsFake)
    {
        _paginatedResultsFake = paginatedResultsFake;

        PupilPremiumSearchResponse response =
            PupilPremiumSearchResponseTestDouble.CreateSuccessResponse();

        _mockUseCase
            .Setup(
                (useCase)
                    => useCase.HandleRequestAsync(
                        It.IsAny<PupilPremiumSearchRequest>()))
            .ReturnsAsync(response);

        _mockLearnerNumberSearchResponseToViewModelMapper
            .Setup(
                (mapper)
                    => mapper.Map(
                        It.IsAny<PupilPremiumLearnerNumericSearchMappingContext>()))
            .Returns(new LearnerNumberSearchViewModel()
            {
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });
    }

    [Fact]
    public async Task PupilPremium_returns_empty_page_when_first_navigated_to()
    {
        // arrange
        PupilPremiumLearnerNumberController sut = GetController();

        // act
        IActionResult result = await sut.PupilPremium(null);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        _mockSelectionManager.Received().Clear();

        AssertAbstractValues(sut, model);

    }

    [Fact]
    public async Task PupilPremium_returns_search_page_when_returned_to()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // act
        IActionResult result = await sut.PupilPremium(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, upns);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(formattedUpns));
    }

    [Fact]
    public async Task PupilPremium_returns_a_page_of_results_when_searched()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));
    }

    [Fact]
    public async Task PupilPremium_returns_another_page_of_results_when_navigated_to()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "");

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(true);
    }

    [Fact]
    public async Task PupilPremium_select_all_works()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["A203102209083"],
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(true);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
        Assert.True(model.ToggleSelectAll);
    }

    [Fact]
    public async Task PupilPremium_deselect_all_works()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = ["A203102209083"]
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
        Assert.False(model.ToggleSelectAll);
    }

    [Fact]
    public async Task PupilPremium_changes_selection_on_page_if_selections_are_different()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = ["A203102209083"],
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(["A203102209083"]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        _mockSelectionManager.Received().AddAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203102209083" })));
        _mockSelectionManager.Received().RemoveAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203202811068" })));
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
    }

    [Fact]
    public async Task PupilPremium_shows_error_if_no_UPNs_inputted()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new();

        PupilPremiumLearnerNumberController sut = GetController();
        sut.ModelState.AddModelError("test", "<span style='display:none'>1</span>");

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        ViewResult? viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(Messages.Search.Errors.EnterUPNs, model.SearchBoxErrorMessage);
    }

    [Fact]
    public async Task PupilPremium_shows_invalid_UPNs_on_search_if_they_exist()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Single(model.Invalid);
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilPremium_shows_not_found_UPNs_on_search_if_they_do_not_exist()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithNotFound();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", false);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Single(model.NotFound);
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task PupilPremium_shows_duplicate_UPNs_on_search_if_they_exist()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithDuplicates();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Single(model.Duplicates);
        Assert.Equal(2, model.Learners.Count());
    }

    //[Fact]
    //public async Task PupilPremium_populates_LearnerNumberIds_with_Id_when_UPN_0()
    //{
    //    // arrange
    //    _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

    //    PupilPremiumLearnerNumberController sut = GetController();

    //    _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
    //    PaginatedResponse response = new()
    //    {
    //        Learners =
    //                [
    //                    new()
    //                    {
    //                        Id = "123",
    //                        LearnerNumber = "0",
    //                    },
    //                    new()
    //                    {
    //                        Id = "456",
    //                        LearnerNumber = "A203202811068",
    //                    }
    //                ],
    //        Count = 2
    //    };
    //    List<Learner> expectedLearners =
    //                [
    //                    new Learner()
    //                    {
    //                       Id = "123",
    //                        LearnerNumber = "0",
    //                        LearnerNumberId = "123",
    //                    },
    //                    new Learner()
    //                    {
    //                        Id = "456",
    //                        LearnerNumber = "A203202811068",
    //                        LearnerNumberId = "A203202811068",
    //                    }
    //                ];

    //    // act
    //    IActionResult result = await sut.PupilPremium(true);

    //    // assert
    //    ViewResult viewResult = Assert.IsType<ViewResult>(result);
    //    LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

    //    Assert.Equal("123\nA203202811068", model.LearnerNumberIds);
    //    Assert.True(model.Learners.SequenceEqual(expectedLearners));
    //}

    [Fact]
    public async Task PupilPremium_preserves_sort_settings_when_navigated_to()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(true);
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_when_select_all_chosen()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["A203102209083"],
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(true);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
        Assert.True(model.ToggleSelectAll);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilUlnSearch_preserves_sort_settings_if_deselect_all_chosen()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = ["A203102209083"]
        };

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        model.Learners.AssertSelected(false);
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(1, model.PageNumber);
        Assert.False(model.ToggleSelectAll);

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_preserves_sort_settings_in_session_if_returnToSearch_true()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        string sortField = "Forename";
        string sortDirection = "asc";
        _mockSession.SetString(sut.SearchSessionSortField, sortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, sortDirection);

        // act
        IActionResult result = await sut.PupilPremium(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, upns);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(formattedUpns));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_forename_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_forename_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Forename";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_middlenames_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "MiddleNames";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_middlenames_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "MiddleNames";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_surname_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Surname";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_surname_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Surname";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_gender_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Gender";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_gender_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Gender";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_dob_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Dob";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task PupilPremium_updates_model_with_sorting_dob_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Dob";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(upns));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(formattedUpns));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task AddToMyPupilList_adds_to_mpl()
    {
        // Arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        // Act
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
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
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
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager
            .GetSelected(Arg.Any<string[]>())
            .Returns([.. formattedUpns]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public async Task AddToMyPupilList_returns_an_error_if_over_limit()
    {
        // Arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);

        _addPupilsUseCaseMock
            .When(t => t.HandleRequestAsync(Arg.Any<AddPupilsToMyPupilsRequest>()))
            .Throws(new MyPupilsLimitExceededException(1));

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            upns);

        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        Assert.Equal(Messages.Common.Errors.MyPupilListLimitExceeded, model.ErrorDetails);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString(
           _paginatedResultsFake.TotalSearchResultsSessionKey,
           _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_with_empty_document_returns_to_search_page_with_errorDetails()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        List<string> formattedUpns = upns.FormatLearnerNumbers().ToList();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = formattedUpns,
            PageLearnerNumbers = string.Join(',', formattedUpns)
        };

        PupilPremiumLearnerNumberController sut = GetController();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([.. formattedUpns]);
        _mockSession.SetString(sut.SearchSessionKey, upns);
        _mockSession.SetString("totalSearch", "20");

        // act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        Assert.Contains(Messages.Downloads.Errors.NoDataForSelectedPupils, model.ErrorDetails);
    }

    private static void AssertAbstractValues(PupilPremiumLearnerNumberController controller, LearnerNumberSearchViewModel model)
    {
        Assert.Equal(ApplicationLabels.SearchPupilPremiumWithUpnPageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.SearchAction, model.SearchAction);
        Assert.Equal(controller.FullTextLearnerSearchController, model.FullTextLearnerSearchController);
        Assert.Equal(controller.FullTextLearnerSearchAction, model.FullTextLearnerSearchAction);
    }

    private PupilPremiumLearnerNumberController GetController()
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumUPNsPerSearch = 4000,
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);
        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));

        // TODO verify serializer called, but will require pulling all of this sut creation out
        Mock<IJsonSerializer> jsonSerializerMock = new();
        List<string>? notFoundLearners = ["E938218618008"];

        jsonSerializerMock
            .Setup((t) => t.Serialize(It.IsAny<object>()))
            .Returns(string.Empty);
        jsonSerializerMock
            .Setup(t => t.TryDeserialize(It.IsAny<string>(), out It.Ref<List<string>?>.IsAny))
            .Returns((string _, out List<string>? value) =>
            {
                value = notFoundLearners;
                return true;
            });

        Mock<IDownloadPupilPremiumPupilDataService> downloadPupilPremiumDataServiceMock = new();

        DownloadPupilPremiumFilesResponse responseStubNoData =
            new(
                new DownloadPupilDataResponse());

        downloadPupilPremiumDataServiceMock
            .Setup(service => service.DownloadAsync(
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<Core.Common.CrossCutting.Logging.Events.DownloadType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(responseStubNoData);

        IReadOnlyList<string> validSortFields = new List<string> { "MockSortField" };
        Mock<IMapper<SortOrderRequest, SortOrder>> mockMapper = new();
        mockMapper
            .Setup((mapper) => mapper.Map(It.IsAny<SortOrderRequest>()))
            .Returns(new SortOrder(validSortFields[0], "asc", validSortFields));

        return new PupilPremiumLearnerNumberController(
            _mockLogger,
            _mockUseCase.Object,
            mockMapper.Object,
            _mockLearnerNumberSearchResponseToViewModelMapper.Object,
            _mockSelectionManager,
            _mockAppOptions,
            _addPupilsUseCaseMock,
            jsonSerializerMock.Object,
            downloadPupilPremiumDataServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession }
            }
        };
    }
}
