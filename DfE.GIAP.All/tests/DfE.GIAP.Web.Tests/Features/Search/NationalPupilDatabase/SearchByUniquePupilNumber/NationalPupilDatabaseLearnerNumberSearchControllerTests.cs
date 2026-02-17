using System.Security.Claims;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.MyPupils.Domain.Exceptions;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Application.Options.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.Models;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.SharedTests.TestDoubles.Learner;
using DfE.GIAP.SharedTests.TestDoubles.SearchIndex;
using DfE.GIAP.Web.Config;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Enums;
using DfE.GIAP.Web.Features.Search.LegacyModels.Learner;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.Helpers;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Shared.Serializer;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.SearchByUniquePupilNumber;

public sealed class NationalPupilDatabaseLearnerNumberSearchControllerTests : IClassFixture<PaginatedResultsFake>
{
    private readonly ILogger<NationalPupilDatabaseLearnerNumberSearchController> _mockLogger = Substitute.For<ILogger<NationalPupilDatabaseLearnerNumberSearchController>>();
    private readonly ISelectionManager _mockSelectionManager = Substitute.For<ISelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private readonly IUseCaseRequestOnly<AddPupilsToMyPupilsRequest> _addPupilsUseCaseMock = Substitute.For<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>();
    private AzureAppSettings _mockAppSettings = new();

    private readonly SessionFake _mockSession = new();

    private readonly PaginatedResultsFake _paginatedResultsFake;

    private readonly Mock<
        IUseCase<
            NationalPupilDatabaseSearchByUniquePupilNumberRequest, SearchResponse<NationalPupilDatabaseLearners>>> _mockUseCase = new();

    private readonly Mock<
        IMapper<
            NationalPupilDatabaseLearnerNumericSearchMappingContext, LearnerNumberSearchViewModel>> _mockLearnerNumberSearchResponseToViewModelMapper = new();

    private readonly Mock<ISearchIndexOptionsProvider> _searchindexOptionsProvider = new();

    private readonly Mock<ISortOrderFactory> _sortOrderFactoryMock = new();

    private readonly Mock<IMapper<SearchCriteriaOptions, SearchCriteria>> _criteriaOptionsToCriteriaMock = new();

    public NationalPupilDatabaseLearnerNumberSearchControllerTests(PaginatedResultsFake paginatedResultsFake)
    {
        _paginatedResultsFake = paginatedResultsFake;

        SortOrder stubSortOrder = new(
            sortField: "Surname",
            sortDirection: "asc",
            validSortFields: ["Surname", "DOB", "Forename"]
        );

        _sortOrderFactoryMock.Setup(
            t => t.Create(
                    It.IsAny<SortOptions>(),
                    It.IsAny<(string?, string?)>()))
                .Returns(stubSortOrder);

        _searchindexOptionsProvider.Setup(
            indexOptionsProvider =>
                indexOptionsProvider.GetOptions(It.IsAny<string>()))
                    .Returns(new SearchIndexOptions());

        _criteriaOptionsToCriteriaMock.Setup(
            criteriaOptionsMapper =>
                criteriaOptionsMapper.Map(
                    It.IsAny<SearchCriteriaOptions>()))
                        .Returns(SearchCriteriaTestDouble.Stub());

        SearchResponse<NationalPupilDatabaseLearners> response = SearchResponse<NationalPupilDatabaseLearners>.Create(
            NationalPupilDatabaseLearners.Create([NationalPupilDatabaseLearnerTestDoubles.Fake()]),
            SearchFacetsTestDouble.Stub(),
            totalResults: 1);

        _mockUseCase
            .Setup(
                (useCase)
                    => useCase.HandleRequestAsync(
                        It.IsAny<NationalPupilDatabaseSearchByUniquePupilNumberRequest>()))
            .ReturnsAsync(response);

        _mockLearnerNumberSearchResponseToViewModelMapper
            .Setup(
                (mapper)
                    => mapper.Map(
                        It.IsAny<NationalPupilDatabaseLearnerNumericSearchMappingContext>()))
            .Returns(new LearnerNumberSearchViewModel()
            {
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });
    }

    [Fact]
    public async Task NationalPupilDatabase_returns_empty_page_when_first_navigated_to()
    {
        // arrange
        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        // act
        IActionResult result = await sut.NationalPupilDatabase(null);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        _mockSelectionManager.Received().Clear();
        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task NationalPupilDatabase_returns_search_page_when_returned_to()
    {
        // arrange
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        // act
        IActionResult result = await sut.NationalPupilDatabase(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, _paginatedResultsFake.GetUpns());
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task NationalPupilDatabase_returns_a_page_of_results_when_searched()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
    }

    [Fact]
    public async Task NationalPupilDatabase_returns_another_page_of_results_when_navigated_to()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            PageLearnerNumbers = string.Join(',', _paginatedResultsFake.GetUpns().FormatLearnerNumbers())
        };

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        // act
        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(true);
    }

    [Fact]
    public async Task NationalPupilDatabase_select_all_works()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["A203102209083"],
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
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
    public async Task NationalPupilDatabase_deselect_all_works()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = ["A203102209083"]
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, "", "");

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
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
    public async Task NationalPupilDatabase_changes_selection_on_page_if_selections_are_different()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = ["A203102209083"],
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(["A203102209083"]);

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        _mockSelectionManager.Received().AddAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203102209083" })));
        _mockSelectionManager.Received().RemoveAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203202811068" })));
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(1, model.PageNumber);
    }

    [Fact]
    public async Task NationalPupilDatabase_shows_error_if_no_UPNs_inputted()
    {
        // arrange
        LearnerNumberSearchViewModel inputModel = new();

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();
        sut.ModelState.AddModelError("test", "<span style='display:none'>1</span>");

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(Messages.Search.Errors.EnterUPNs, model.SearchBoxErrorMessage);
    }

    [Fact]
    public async Task NationalPupilDatabase_shows_invalid_UPNs_on_search_if_they_exist()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = upns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Single(model.Invalid);
        Assert.Equal(2, model.Learners.Count());
    }


    [Fact]
    public async Task NationalPupilDatabase_shows_not_found_UPNs_on_search_if_they_do_not_exist()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithNotFound();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = upns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, "", "", false);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Single(model.NotFound);
        Assert.Equal(2, model.Learners.Count());
    }

    [Fact]
    public async Task NationalPupilDatabase_shows_duplicate_UPNs_on_search_if_they_exist()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithDuplicates();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = upns.FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Single(model.Duplicates);
        Assert.Equal(2, model.Learners.Count());
    }

    //[Fact]
    //public async Task NationalPupilDatabase_populates_LearnerNumberIds_with_Id_when_UPN_0()
    //{
    //    // arrange
    //    _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

    //    NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

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
    //    SetupPaginatedSearch(sut.IndexType, response);

    //    // act
    //    IActionResult result = await sut.NationalPupilDatabase(true);

    //    // assert

    //    ViewResult viewResult = Assert.IsType<ViewResult>(result);
    //    LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

    //    Assert.Equal("123\nA203202811068", model.LearnerNumberIds);
    //    Assert.True(model.Learners.SequenceEqual(expectedLearners));
    //}

    [Fact]
    public async Task NationalPupilDatabase_preserves_sort_settings_when_navigated_to()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            PageLearnerNumbers = string.Join(',', _paginatedResultsFake.GetUpns().FormatLearnerNumbers())
        };

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
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
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "true",
            SelectedPupil = ["A203102209083"],
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
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
        string upns = _paginatedResultsFake.GetUpns();

        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectAllNoJsChecked = "false",
            SelectedPupil = ["A203102209083"]
        };

        PaginatedResponse paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 1, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
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
    public async Task NationalPupilDatabase_preserves_sort_settings_in_session_if_returnToSearch_true()
    {
        // arrange
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());
        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        string sortField = "Forename";
        string sortDirection = "asc";
        _mockSession.SetString(sut.SearchSessionSortField, sortField);
        _mockSession.SetString(sut.SearchSessionSortDirection, sortDirection);

        // act
        IActionResult result = await sut.NationalPupilDatabase(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, _paginatedResultsFake.GetUpns());
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_forename_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_forename_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Forename";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_middlenames_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "MiddleNames";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_middlenames_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "MiddleNames";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_surname_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Surname";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_surname_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Surname";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_sex_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Sex";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_gender_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Sex";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_dob_asc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Dob";
        string sortDirection = "asc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel? model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

    [Fact]
    public async Task NationalPupilDatabase_updates_model_with_sorting_dob_desc_correctly()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        string sortField = "Dob";
        string sortDirection = "desc";

        // act
        IActionResult result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        Assert.Equal(model.SortField, sortField);
        Assert.Equal(model.SortDirection, sortDirection);
    }

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

        _mockSelectionManager
            .GetSelected(Arg.Any<string[]>())
            .Returns(upns.FormatLearnerNumbers().ToHashSet());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            _paginatedResultsFake.GetUpns());

        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // Act
        IActionResult result = await sut.NPDAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
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

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(
            sut.SearchSessionKey,
            _paginatedResultsFake.GetUpns());

        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // Act
        IActionResult result = await sut.NPDAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
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

        _mockSelectionManager
            .GetSelected(Arg.Any<string[]>())
            .Returns(upns.FormatLearnerNumbers().ToHashSet());

        _addPupilsUseCaseMock
            .When(t => t.HandleRequestAsync(Arg.Any<AddPupilsToMyPupilsRequest>()))
            .Throws(new MyPupilsLimitExceededException(1));

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // Act
        IActionResult result = await sut.NPDAddToMyPupilList(inputModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(Global.SearchView, viewResult.ViewName);
        Assert.Equal(Messages.Common.Errors.MyPupilListLimitExceeded, model.ErrorDetails);
    }


    [Fact]
    public async Task DownloadCommonTransferFileData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.DownloadCommonTransferFileData(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }


    [Fact]
    public async Task DownloadCommonTransferFileData_exceeding_commonTransferFileUPNLimit_returns_to_search_page_with_errorDetails()
    {
        // arrange

        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController(commonTransferFileUPNLimit: 1);
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());
        _mockSession.SetString("totalSearch", "20");

        // act
        IActionResult result = await sut.DownloadCommonTransferFileData(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Contains(Messages.Downloads.Errors.UPNLimitExceeded, model.ErrorDetails);
    }

    [Fact]
    public async Task ToDownloadSelectedNPDDataUpn_returns_to_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns([]);

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        _mockSession.SetString(
            _paginatedResultsFake.TotalSearchResultsSessionKey,
            _paginatedResultsFake.TotalSearchResultsSessionValue);

        // act
        IActionResult result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.SearchView, viewResult.ViewName);

        LearnerNumberSearchViewModel model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task ToDownloadSelectedNPDDataUpn_returns_options_page_when_pupils_selected()
    {
        string upns = _paginatedResultsFake.GetUpns();
        LearnerNumberSearchViewModel inputModel = new()
        {
            LearnerNumberIds = upns,
            LearnerNumber = upns,
            SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
        };

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        string joinedSelectedPupils = string.Join(',', upns.FormatLearnerNumbers());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // act
        IActionResult result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        Assert.Equal(model.SelectedPupils, joinedSelectedPupils);
        Assert.Equal(upns.FormatLearnerNumbers().Length, model.SelectedPupilsCount);
        Assert.Equal(model.LearnerNumber, upns);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_redirects_to_npd_search_if_SelectedPupils_empty()
    {
        // arrange
        LearnerDownloadViewModel inputDownloadModel = new();

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // act
        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        RedirectToActionResult redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(Global.NPDLearnerNumberSearchAction, redirectResult.ActionName);
        Assert.Equal(Global.NPDLearnerNumberSearchController, redirectResult.ControllerName);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_type_selected()
    {
        string[] upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', upns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        Assert.Equal(joinedSelectedPupils, model.SelectedPupils);
        Assert.Equal(upns.Length, model.SelectedPupilsCount);
        Assert.Equal(Messages.Search.Errors.SelectOneOrMoreDataTypes, sut.TempData["ErrorDetails"]);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_download_type_selected()
    {
        string[] upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        string joinedSelectedPupils = string.Join(',', upns);

        LearnerDownloadViewModel inputDownloadModel = new()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length,
            SelectedDownloadOptions = [],
            DownloadFileType = DownloadFileType.None
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        NationalPupilDatabaseLearnerNumberSearchController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.DownloadNPDOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        Assert.Equal(joinedSelectedPupils, model.SelectedPupils);
        Assert.Equal(upns.Length, model.SelectedPupilsCount);
        Assert.Equal(Messages.Search.Errors.SelectFileType, sut.TempData["ErrorDetails"]);
    }

    private static void AssertAbstractValues(NationalPupilDatabaseLearnerNumberSearchController controller, LearnerNumberSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.SearchAction, model.SearchAction);
        Assert.Equal(controller.FullTextLearnerSearchController, model.FullTextLearnerSearchController);
        Assert.Equal(controller.FullTextLearnerSearchAction, model.FullTextLearnerSearchAction);
    }

    private NationalPupilDatabaseLearnerNumberSearchController GetController(int commonTransferFileUPNLimit = 4000)
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            CommonTransferFileUPNLimit = commonTransferFileUPNLimit,
            DownloadOptionsCheckLimit = 500
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);
        _mockSession.SetString(NationalPupilDatabaseLearnerNumberSearchController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));


        List<AvailableDatasetResult> availableDatasetResults =
            [
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.KS1, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.KS2, HasData: true, CanDownload: true)
            ];
        GetAvailableDatasetsForPupilsResponse availableDatasetResponse = new(availableDatasetResults);
        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(availableDatasetResponse);

        Mock<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>> mockDownloadPupilDataUseCase = new();
        mockDownloadPupilDataUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<DownloadPupilDataRequest>()))
            .ReturnsAsync(It.IsAny<DownloadPupilDataResponse>);
        Mock<IEventLogger> mockEventLogger = new();

        // TODO verify serializer called, but will require pulling all of this sut creation out
        Mock<IJsonSerializer> jsonSerializerMock = new();
        List<string>? notFoundLearners = ["E938218618008"];

        jsonSerializerMock
            .Setup(t => t.Serialize(It.IsAny<object>()))
            .Returns(string.Empty);
        jsonSerializerMock
            .Setup(t => t.TryDeserialize(It.IsAny<string>(), out It.Ref<List<string>?>.IsAny))
            .Returns((string _, out List<string>? value) =>
            {
                value = notFoundLearners;
                return true;
            });

        return new NationalPupilDatabaseLearnerNumberSearchController(
            _mockLogger,
            _mockUseCase.Object,
            _sortOrderFactoryMock.Object,
            _mockLearnerNumberSearchResponseToViewModelMapper.Object,
            _mockSelectionManager,
            _mockAppOptions,
            _addPupilsUseCaseMock,
            mockGetAvailableDatasetsForPupilsUseCase.Object,
            jsonSerializerMock.Object,
            mockDownloadPupilDataUseCase.Object,
            mockEventLogger.Object,
            _searchindexOptionsProvider.Object,
            _criteriaOptionsToCriteriaMock.Object,
            new Mock<IUseCase<DownloadPupilCtfRequest, DownloadPupilCtfResponse>>().Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession }
            }
        };
    }
}
