using System.Security.Claims;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.FurtherEducation;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.TestDoubles;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;
using static DfE.GIAP.Web.Features.Search.FurtherEducation.SearchByName.FurtherEducationLearnerTextSearchResponseToViewModelMapper;

namespace DfE.GIAP.Web.Tests.Features.Search.FurtherEducation.SearchByName;

public sealed class FELearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
{

    private readonly ISessionProvider _sessionProvider = Substitute.For<ISessionProvider>();
    private readonly ILogger<FELearnerTextSearchController> _mockLogger = Substitute.For<ILogger<FELearnerTextSearchController>>();
    private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
    private readonly ITempDataProvider _mockTempDataProvider = Substitute.For<ITempDataProvider>();
    private readonly SessionFake _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly SearchFiltersFakeData _searchFiltersFake;

    private readonly IUseCase<FurtherEducationSearchRequest, FurtherEducationSearchResponse> _mockUseCase =
        Substitute.For<IUseCase<FurtherEducationSearchRequest, FurtherEducationSearchResponse>>();

    private readonly IMapper<FurtherEducationLearnerTextSearchMappingContext, LearnerTextSearchViewModel> _mockLearnerSearchResponseToViewModelMapper =
        Substitute.For<IMapper<FurtherEducationLearnerTextSearchMappingContext, LearnerTextSearchViewModel>>();

    private readonly IMapper<Dictionary<string, string[]>, IList<FilterRequest>> _mockFiltersRequestMapper =
        Substitute.For<IMapper<Dictionary<string, string[]>, IList<FilterRequest>>>();

    private readonly IFiltersRequestFactory _mockFiltersRequestBuilder = Substitute.For<IFiltersRequestFactory>();

    private readonly IMapper<SortOrderRequest, SortOrder> _mockSortOrderMapper =
        Substitute.For<IMapper<SortOrderRequest, SortOrder>>();

    public FELearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
        _searchFiltersFake = searchFiltersFake;

        SortOrder stubSortOrder = new(
            sortField: "Surname",
            sortDirection: "asc",
            validSortFields: ["Surname", "DOB", "Forename"]
        );

        _mockSortOrderMapper.Map(
            Arg.Any<SortOrderRequest>()).Returns(stubSortOrder);

        FurtherEducationSearchResponse response =
            FurtherEducationSearchResponseTestDouble.CreateSuccessResponse();

        _mockUseCase.HandleRequestAsync(
            Arg.Any<FurtherEducationSearchRequest>()).Returns(response);

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = "Somethuiing",
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });
    }

    [Fact]
    public async Task FurtherEducationNonUlnSearch_returns_empty_page_when_first_navigated_to()
    {
        // arrange
        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.FurtherEducationNonUlnSearch(null);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);

        _mockSelectionManager.Received().Clear();
        AssertAbstractValues(sut, model);
        Assert.True(string.IsNullOrEmpty(model.SearchText));
    }
    [Fact]
    public async Task FurtherEducationNonUlnSearch_clears_search_when_return_to_search_is_false()
    {
        // arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        FELearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        // act
        IActionResult result = await sut.FurtherEducationNonUlnSearch(false);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);

        Assert.True(string.IsNullOrEmpty(model.SearchText));
        Assert.False(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task FurtherEducationNonUlnSearch_return_to_search_page_persists_search()
    {
        // arrange
        string searchText = "John Smith";
        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
                PageHeading = "Advanced search ULN",
                DownloadLinksPartial = "~/Views/Shared/LearnerText/_SearchFurtherEducationDownloadLinks.cshtml",
                LearnerTextSearchController = "FELearnerTextSearch",
                LearnerTextSearchAction = "FurtherEducationNonUlnSearch",
                LearnerNumberController = "search",
                LearnerNumberAction = "pupil-uln"
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        // act
        IActionResult result = await sut.FurtherEducationNonUlnSearch(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task FurtherEducationNonUlnSearch_return_to_search_sets_download_link()
    {
        // arrange
        string searchText = "John Smith";
        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
                DownloadSelectedLink = "Download FE data"
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        // act
        IActionResult result = await sut.FurtherEducationNonUlnSearch(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedFurtherEducationLink, model.DownloadSelectedLink);
    }


    [Theory]
    [ClassData(typeof(DobSearchFilterTestData))]
    public async Task DobFilter_Adds_DOB_month_and_year_filter_as_expected(SearchFilters searchFilter)
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);
        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilter,
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.DobDay, searchViewModel.SearchFilters.CustomFilterText.DobDay);
        Assert.Equal(model.SearchFilters.CustomFilterText.DobMonth, searchViewModel.SearchFilters.CustomFilterText.DobMonth);
        Assert.Equal(model.SearchFilters.CustomFilterText.DobYear, searchViewModel.SearchFilters.CustomFilterText.DobYear);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorEmpty()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(0, 0, 0);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(searchViewModel.FilterErrors.DobErrorEmpty);
        Assert.True(searchViewModel.FilterErrors.DobError);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorDayOnly()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 0, 0);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorDayOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorDayMonthOnly()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 1, 0);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorDayMonthOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DayOutOfRange()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(99, 1, 2015);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DayOutOfRange);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorMonthOnly()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(0, 1, 0);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorMonthOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_MonthOutOfRange()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 99, 2015);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.MonthOutOfRange);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_YearLimitHigh()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 2, 9999);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.YearLimitHigh);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_YearLimitLow()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 2, 1970);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.YearLimitLow);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task SurnameFilter_Returns_to_route_with_correct_surname_filter()
    {
        // Arrange
        string searchText = "John Smith";
        string surnameFilter = "Surname";

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        string sortField = "Forename";
        string sortDirection = "asc";

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
                SortField = sortField,
                SortDirection = sortDirection
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Surname, searchViewModel.SearchFilters.CustomFilterText.Surname);
    }

    [Fact]
    public async Task ForneameFilter_Returns_to_route_with_correct_forename_filter()
    {
        // Arrange
        const string searchText = "John Smith";
        const string forenameFilter = "Forename";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        searchFilters.CurrentFiltersAppliedString =
            "[{\"FilterName\":\"1/1/2015\",\"FilterType\":3},{\"FilterName\":\"forename\",\"FilterType\":2}]";
        searchFilters.CustomFilterText.Forename = "Forename";

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        FELearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.ForenameFilter(searchViewModel, forenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Forename, searchViewModel.SearchFilters.CustomFilterText.Forename);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("O")]
    public async Task SexFilter_Returns_to_route_with_correct_sex_filter(string sexFilter)
    {
        // Arrange
        const string searchText = "John Smith";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                SelectedSexValues = [sexFilter],
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SelectedSexValues[0], sexFilter);
    }

    [Fact]
    public async Task SexFilter_returns_all_sexes_when_no_sex_selected()
    {
        // Arrange
        const string searchText = "Smith";

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        searchFilters.CurrentFiltersAppliedString =
            @"[{ ""FilterName"":""Female"",""FilterType"":6}]";

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedSexValues);
    }

    [Fact]
    public async Task SexFilter_returns_all_sexes_when_more_than_one_sex_deselected()
    {
        // Arrange
        const string searchText = "Smith";
        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        searchFilters.CurrentFiltersAppliedString =
            @"[{""FilterName"":""Female"",""FilterType"":6}, {""FilterName"":""Male"",""FilterType"":6}]";

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedSexValues);
    }


    [Theory]
    [InlineData("Forename", "asc")]
    [InlineData("Surname", "desc")]
    public async Task Sort_is_correctly_handled(string sortField, string sortDirection)
    {
        // arrange

        string searchText = "John Smith";
        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
                SortDirection = sortDirection,
                SortField = sortField
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        string surnameFilter = null!;
        string middlenameFilter = null!;
        string forenameFilter = null!;
        string searchByRemove = null!;

        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        // act
        IActionResult result =
            await sut.FurtherEducationNonUlnSearch(
                searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, sortField, sortDirection);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Equal(sortField, model.SortField);
        Assert.Equal(sortDirection, model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_remembered_when_page_number_moves()
    {
        // arrange
        string searchText = "John Smith";

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        string surnameFilter = null!;
        string middlenameFilter = null!;
        string forenameFilter = null!;
        string searchByRemove = null!;

        string sortField = "Forename";
        string sortDirection = "asc";

        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SortDirectionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SortDirectionKey)).Returns(sortDirection);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SortFieldKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SortFieldKey)).Returns(sortField);

        // act
        IActionResult result =
            await sut.FurtherEducationNonUlnSearch(
                searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Equal(sortField, model.SortField);
        Assert.Equal(sortDirection, model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_remembered_when_returning_to_search()
    {
        // arrange
        string searchText = "John Smith";

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        string sortField = "Forename";
        string sortDirection = "asc";

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners,
                PageHeading = "Advanced search ULN",
                DownloadLinksPartial = "~/Views/Shared/LearnerText/_SearchFurtherEducationDownloadLinks.cshtml",
                LearnerTextSearchController = "FELearnerTextSearch",
                LearnerTextSearchAction = "FurtherEducationNonUlnSearch",
                LearnerNumberController = "search",
                LearnerNumberAction = "pupil-uln",
                SortField = sortField,
                SortDirection = sortDirection
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SortDirectionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SortDirectionKey)).Returns(sortDirection);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SortFieldKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SortFieldKey)).Returns(sortField);

        // act
        IActionResult result = await sut.FurtherEducationNonUlnSearch(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Equal(sortField, model.SortField);
        Assert.Equal(sortDirection, model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_cleared_when_page_is_reset()
    {
        // arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        string surnameFilter = null!;
        string middlenameFilter = null!;
        string forenameFilter = null!;
        string searchByRemove = null!;
        string sortField = "Forename";
        string sortDirection = "asc";

        FELearnerTextSearchController sut = GetController();
        _sessionProvider.SetSessionValue(sut.SearchSessionKey, searchText);
        _sessionProvider.SetSessionValue(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));
        _sessionProvider.SetSessionValue(sut.SortDirectionKey, sortDirection);
        _sessionProvider.SetSessionValue(sut.SortFieldKey, sortField);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        // act
        IActionResult result = await sut.FurtherEducationNonUlnSearch(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Null(model.SortField);
        Assert.Null(model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_cleared_when_filters_are_removed()
    {
        // Arrange
        const string searchText = "John Smith";
        const string surnameFilter = "";
        const string middlenameFilter = null!;
        const string forenameFilter = null!;
        const string searchByRemove = "Male";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText, _searchFiltersFake.GetSearchFilters());

        ITempDataDictionary mockTempDataDictionary = Substitute.For<ITempDataDictionary>();
        FELearnerTextSearchController sut = GetController();
        sut.TempData = mockTempDataDictionary;

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        // act
        IActionResult result =
            await sut.FurtherEducationNonUlnSearch(
                searchViewModel,
                surnameFilter,
                middlenameFilter,
                forenameFilter,
                searchByRemove,
                string.Empty,
                string.Empty);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.True(string.IsNullOrEmpty(model.SortField));
        Assert.True(string.IsNullOrEmpty(model.SortDirection));
    }


    /*    private LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(string searchText, SearchFilters searchFilters, string[] selectedGenderValues = null)
        {
            return new LearnerTextSearchViewModel()
            {

                };

            _mockLearnerSearchResponseToViewModelMapper.Map(
                Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

            // act
            FELearnerTextSearchController sut = GetController();




            _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
            _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
                .Returns(searchViewModel.SearchFilters);

            var result = await sut.FurtherEducationNonUlnSearch(true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

            Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerTextSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(searchText, model.SearchText);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        }*/

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorNoMonth()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 0, 2015);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        FELearnerTextSearchController sut = GetController();

        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorNoMonth);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }


    [Fact]
    public async Task ToDownloadSelectedFEDataULN_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        string upn = string.Empty;
        string searchText = "John Smith";
        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        LearnerTextSearchViewModel searchViewModel =
            new()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<FurtherEducationLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        FELearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.ToDownloadSelectedFEDataULN(searchViewModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Theory]
    [InlineData(DownloadFileType.None, new string[] { "csv" }, new byte[0], Messages.Search.Errors.SelectFileType)]
    [InlineData(DownloadFileType.CSV, null, new byte[0], Messages.Search.Errors.SelectOneOrMoreDataTypes)]
    [InlineData(DownloadFileType.CSV, new string[] { "csv" }, null, Messages.Downloads.Errors.NoDataForSelectedPupils)]
    public async Task DownloadSelectedFEDataULN_returns_correct_validation_error_message(
        DownloadFileType downloadFileType,
        string[]? selectedDownloadOptions,
        byte[]? fileBytes,
        string errorMessage)
    {
        // arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = downloadFileType,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = selectedDownloadOptions
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        Assert.Equal(errorMessage, model.ErrorDetails);
    }

    [Fact]
    public async Task DownloadSelectedFEDataULN_redirects_to_error_when_file_isNull()
    {
        // arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = ["csv"]
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task DownloadSelectedFEDataULN_redirects_to_error_when_learnerNumber_isNull()
    {
        // arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = null,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = ["csv"]
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        FELearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);

    }

    [Fact]
    public async Task Sort_is_cleared_when_filters_are_set()
    {
        // Arrange
        string searchText = "John Smith";
        string surnameFilter = "Surname";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        FELearnerTextSearchController sut = GetController();

        //_mockSession.SetString(sut.SortDirectionKey, "asc");
        //_mockSession.SetString(sut.SortFieldKey, "Forename");
        _sessionProvider.SetSessionValue(sut.SortDirectionKey, "asc");
        _sessionProvider.SetSessionValue(sut.SortFieldKey, "Forename");

        // act
        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Null(model.SortField);
        Assert.Null(model.SortDirection);
    }

    private LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(
        string searchText,
        SearchFilters searchFilters)
    {
        return new()
        {
            SearchText = searchText,
            SearchFilters = searchFilters,
            Learners = _paginatedResultsFake.GetValidLearners().Learners,
            PageHeading = "Advanced search ULN",
            DownloadLinksPartial = "~/Views/Shared/LearnerText/_SearchFurtherEducationDownloadLinks.cshtml",
            LearnerTextSearchController = "FELearnerTextSearch",
            LearnerTextSearchAction = "NonUpnPupilPremiumDatabase",
            LearnerNumberController = "search",
            LearnerNumberAction = "NonUpnPupilPremiumDatabase"
        };
    }

    private static void AssertAbstractValues(FELearnerTextSearchController controller, LearnerTextSearchViewModel model)
    {
        Assert.Equal(ApplicationLabels.SearchFEWithoutUlnPageHeading, model.PageHeading);
        Assert.Equal(Global.FENonUlnDownloadLinksView, model.DownloadLinksPartial);
        Assert.Equal(Global.FELearnerTextSearchController, model.LearnerTextSearchController);
        Assert.Equal(Global.FELearnerTextSearchAction, model.LearnerTextSearchAction);
        Assert.Equal(Routes.Application.Search, model.LearnerNumberController);
        Assert.Equal(Routes.FurtherEducation.LearnerNumberSearch, model.LearnerNumberAction);
    }


    private FELearnerTextSearchController GetController()
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetFEApproverClaimsPrincipal();

        DefaultHttpContext httpContextStub = new()
        {
            User = user,
            Session = new Mock<ISession>().Object
        };
        TempDataDictionary mockTempData = new(httpContextStub, _mockTempDataProvider);
        Mock<IEventLogger> mockEventLogger = new();

        List<AvailableDatasetResult> availableDatasetResults =
            [
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.PP, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.SEN, HasData: true, CanDownload: true)
            ];
        GetAvailableDatasetsForPupilsResponse response = new(availableDatasetResults);

        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(response);

        DownloadPupilDataResponse downloadPupilDataResponse = new();
        Mock<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>> mockDownloadPupilDataUseCase = new();
        mockDownloadPupilDataUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<DownloadPupilDataRequest>()))
            .ReturnsAsync(downloadPupilDataResponse);

        return new FELearnerTextSearchController(
            _sessionProvider,
            _mockUseCase,
            _mockLearnerSearchResponseToViewModelMapper,
            _mockFiltersRequestMapper,
            _mockSortOrderMapper,
            _mockFiltersRequestBuilder,
            _mockLogger,
            _mockSelectionManager,
            mockEventLogger.Object,
            mockGetAvailableDatasetsForPupilsUseCase.Object,
            mockDownloadPupilDataUseCase.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextStub
            },
            TempData = mockTempData
        };
    }

    private SearchFilters SetDobFilters(int day, int month, int year)
    {
        return new SearchFilters()
        {
            CurrentFiltersApplied =
                [
                    new CurrentFilterDetail()
                    {
                        FilterType = FilterType.Dob,
                        FilterName = $"{day}/{month}/{year}"
                    }
                ],
            CurrentFiltersAppliedString = "[{\"FilterName\":\"" + day + "/" + month + "/" + year + "\",\"FilterType\":3}]",
            CustomFilterText = new CustomFilterText()
            {
                DobDay = day,
                DobMonth = month,
                DobYear = year
            }
        };
    }
}
