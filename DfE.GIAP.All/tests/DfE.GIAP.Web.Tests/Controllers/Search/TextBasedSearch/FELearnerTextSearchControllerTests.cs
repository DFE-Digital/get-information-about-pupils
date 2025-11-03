using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Models.Common;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.UseCases.Request;
using DfE.GIAP.Core.Search.Application.UseCases.Response;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.TextBasedSearch;
using DfE.GIAP.Web.Controllers.TextBasedSearch.Filters;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
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
using static DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers.LearnerTextSearchResponseToViewModelMapper;

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch;

public class FELearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
{
    private readonly ISessionProvider _sessionProvider = Substitute.For<ISessionProvider>();
    private readonly ILogger<FELearnerTextSearchController> _mockLogger = Substitute.For<ILogger<FELearnerTextSearchController>>();
    private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
    private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
    private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private readonly ITempDataProvider _mockTempDataProvider = Substitute.For<ITempDataProvider>();
    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly SearchFiltersFakeData _searchFiltersFake;
    private readonly IUseCase<SearchRequest, SearchResponse> _mockUseCase =
        Substitute.For<IUseCase<SearchRequest, SearchResponse>>();
    private readonly IMapper<LearnerTextSearchMappingContext, LearnerTextSearchViewModel> _mockLearnerSearchResponseToViewModelMapper =
        Substitute.For<IMapper<LearnerTextSearchMappingContext, LearnerTextSearchViewModel>>();
    private readonly IMapper<Dictionary<string, string[]>, IList<FilterRequest>> _mockFiltersRequestMapper =
        Substitute.For<IMapper<Dictionary<string, string[]>, IList<FilterRequest>>>();
    private readonly IFiltersRequestFactory _mockFiltersRequestBuilder = Substitute.For<IFiltersRequestFactory>();
    private readonly IMapper<(string, string), SortOrder> _mockSortOrderMapper =
        Substitute.For<IMapper<(string, string), SortOrder>>();

    private readonly IMyPupilListService _mockMplService = Substitute.For<IMyPupilListService>();
    private readonly TestSession _mockSession = new TestSession();
    private AzureAppSettings _mockAppSettings = new();

    public FELearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
    {

        _paginatedResultsFake = paginatedResultsFake;
        _searchFiltersFake = searchFiltersFake;

        SortOrder stubSortOrder = new(
            sortField: "Surname",
            sortDirection: "asc",
            validSortFields: new[] { "Surname", "DOB", "Forename" }
        );

        _mockSortOrderMapper.Map(
            Arg.Any<(string, string)>()).Returns(stubSortOrder);

        SearchResponse response =
            SearchByKeyWordsResponseTestDouble.CreateSuccessResponse();

        _mockUseCase.HandleRequestAsync(
            Arg.Any<SearchRequest>()).Returns(response);

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(
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
        SetupContentServicePublicationSchedule();

        // act
        var sut = GetController();
        var result = await sut.FurtherEducationNonUlnSearch(null);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        _mockSelectionManager.Received().Clear();
        AssertAbstractValues(sut, model);
        Assert.True(string.IsNullOrEmpty(model.SearchText));
    }
    [Fact]
    public async Task FurtherEducationNonUlnSearch_clears_search_when_return_to_search_is_false()
    {
        // arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        // act
        var sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.FurtherEducationNonUlnSearch(false);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);

        Assert.True(string.IsNullOrEmpty(model.SearchText));
        Assert.False(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task FurtherEducationNonUlnSearch_return_to_search_page_persists_search()
    {
        // arrange
        SetupContentServicePublicationSchedule();
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
                InvalidUPNsConfirmationAction = "",
                LearnerTextSearchController = "FELearnerTextSearch",
                LearnerTextSearchAction = "FurtherEducationNonUlnSearch",
                LearnerNumberController = "search",
                LearnerNumberAction = "pupil-uln"
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // act
        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        var result = await sut.FurtherEducationNonUlnSearch(true);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task FurtherEducationNonUlnSearch_return_to_search_sets_download_link()
    {
        // arrange
        SetupContentServicePublicationSchedule();
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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // act
        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.FurtherEducationNonUlnSearch(true);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedFurtherEducationLink, model.DownloadSelectedLink);
    }

    [Fact]
    public async Task FurtherEducationNonUlnSearch_does_not_call_GetPage_if_model_state_not_valid()
    {
        // arrange
        SetupContentServicePublicationSchedule();

        // act
        var sut = GetController();

        await sut.FurtherEducationNonUlnSearch(new LearnerTextSearchViewModel(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        // assert
        await _mockPaginatedService.DidNotReceive().GetPage(Arg.Any<string>(),
        Arg.Any<Dictionary<string, string[]>>(),
        Arg.Any<int>(),
        Arg.Any<int>(),
        Arg.Any<AzureSearchIndexType>(),
        Arg.Any<AzureSearchQueryType>(),
        Arg.Any<AzureFunctionHeaderDetails>(),
        Arg.Any<string>(),
        Arg.Any<string>());
    }

    [Theory]
    [ClassData(typeof(DobSearchFilterTestData))]
    public async Task DobFilter_Adds_DOB_month_and_year_filter_as_expected(SearchFilters searchFilter)
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);
        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilter,
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        // act
        FELearnerTextSearchController sut = GetController();
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel? model = viewResult.Model as LearnerTextSearchViewModel;
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
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(0, 0, 0);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        Assert.True(searchViewModel.FilterErrors.DobErrorEmpty);
        Assert.True(searchViewModel.FilterErrors.DobError);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorDayOnly()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(1, 0, 0);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorDayOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorDayMonthOnly()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(1, 1, 0);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorDayMonthOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DayOutOfRange()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(99, 1, 2015);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DayOutOfRange);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorMonthOnly()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(0, 1, 0);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorMonthOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_MonthOutOfRange()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(1, 99, 2015);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.MonthOutOfRange);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_YearLimitHigh()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(1, 2, 9999);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.YearLimitHigh);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_YearLimitLow()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(1, 2, 1970);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.YearLimitLow);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task SurnameFilter_Returns_to_route_with_correct_surname_filter()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // act
        FELearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel? model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Surname, searchViewModel.SearchFilters.CustomFilterText.Surname);
    }

    [Fact]
    public async Task ForneameFilter_Returns_to_route_with_correct_forename_filter()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var forenameFilter = "Forename";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        searchFilters.CurrentFiltersAppliedString =
            "[{\"FilterName\":\"1/1/2015\",\"FilterType\":3},{\"FilterName\":\"forename\",\"FilterType\":2}]";
        searchFilters.CustomFilterText.Forename = "Forename";

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        // act
        var sut = GetController();

        var result = await sut.ForenameFilter(searchViewModel, forenameFilter);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Forename, searchViewModel.SearchFilters.CustomFilterText.Forename);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("O")]
    public async Task GenderFilter_Returns_to_route_with_correct_gender_filter(string genderFilter)
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        string searchText = "John Smith";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                SelectedGenderValues = [genderFilter],
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.GenderFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.True(model.SelectedGenderValues[0].Equals(genderFilter));
    }

    [Fact]
    public async Task GenderFilter_returns_all_genders_when_no_gender_selected()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "Smith";

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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.GenderFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedGenderValues);
    }

    [Fact]
    public async Task GenderFilter_returns_all_genders_when_more_than_one_gender_deselected()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        string searchText = "Smith";
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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.GenderFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedGenderValues);
    }

    [Fact]
    public async Task DownloadSelectedFEDataULN_returns_data_when_SearchSessionKey_is_present()
    {
        // arrange
        var upn = _paginatedResultsFake.GetUpn();
        var downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = new string[] { "csv" }
        };

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
        var sut = GetController();

        var result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    [Theory]
    [InlineData("Forename", "asc")]
    [InlineData("Surname", "desc")]
    public async Task Sort_is_correctly_handled(string sortField, string sortDirection)
    {
        // arrange
        SetupContentServicePublicationSchedule();
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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        string surnameFilter = null!;
        string middlenameFilter = null!;
        string forenameFilter = null!;
        string searchByRemove = null!;

        // act
        FELearnerTextSearchController sut = GetController();

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchSessionKey)).Returns(true);
        _sessionProvider.GetSessionValue(Arg.Is(sut.SearchSessionKey)).Returns(searchText);

        _sessionProvider.ContainsSessionKey(Arg.Is(sut.SearchFiltersSessionKey)).Returns(true);
        _sessionProvider.GetSessionValueOrDefault<SearchFilters>(Arg.Is(sut.SearchFiltersSessionKey))
            .Returns(searchViewModel.SearchFilters);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result =
            await sut.FurtherEducationNonUlnSearch(
                searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, sortField, sortDirection);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel? model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Equal(sortField, model.SortField);
        Assert.Equal(sortDirection, model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_remembered_when_page_number_moves()
    {
        // arrange
        SetupContentServicePublicationSchedule();
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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        string surnameFilter = null!;
        string middlenameFilter = null!;
        string forenameFilter = null!;
        string searchByRemove = null!;

        string sortField = "Forename";
        string sortDirection = "asc";

        // act
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

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result =
            await sut.FurtherEducationNonUlnSearch(
                searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel? model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Equal(sortField, model.SortField);
        Assert.Equal(sortDirection, model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_remembered_when_returning_to_search()
    {
        // arrange
        SetupContentServicePublicationSchedule();
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
                InvalidUPNsConfirmationAction = "",
                LearnerTextSearchController = "FELearnerTextSearch",
                LearnerTextSearchAction = "FurtherEducationNonUlnSearch",
                LearnerNumberController = "search",
                LearnerNumberAction = "pupil-uln",
                SortField = sortField,
                SortDirection = sortDirection
            };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // act
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

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.FurtherEducationNonUlnSearch(true);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel? model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Equal(sortField, model.SortField);
        Assert.Equal(sortDirection, model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_cleared_when_page_is_reset()
    {
        // arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(
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

        // act
        var sut = GetController();
        //_mockSession.SetString(sut.SearchSessionKey, searchText);
        //_mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));
        _sessionProvider.SetSessionValue(sut.SearchSessionKey, searchText);
        _sessionProvider.SetSessionValue(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));
        //_mockSession.SetString(sut.SortDirectionKey, sortDirection);
        //_mockSession.SetString(sut.SortFieldKey, sortField);
        _sessionProvider.SetSessionValue(sut.SortDirectionKey, sortDirection);
        _sessionProvider.SetSessionValue(sut.SortFieldKey, sortField);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.FurtherEducationNonUlnSearch(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.True(viewResult.ViewName.Equals(Global.NonUpnSearchView));

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.Null(model.SortField);
        Assert.Null(model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_cleared_when_filters_are_removed()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        const string searchText = "John Smith";
        const string surnameFilter = "";
        const string middlenameFilter = null;
        const string forenameFilter = null;
        const string searchByRemove = "Male";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText, _searchFiltersFake.GetSearchFilters(), selectedGenderValues: new string[] { "M" });

        ITempDataDictionary mockTempDataDictionary = Substitute.For<ITempDataDictionary>();
        mockTempDataDictionary.Add("PersistedSelectedGenderFilters", searchByRemove);
        FELearnerTextSearchController sut = GetController();
        sut.TempData = mockTempDataDictionary;

        // act
        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.FurtherEducationNonUlnSearch(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, "", "");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        Assert.True(String.IsNullOrEmpty(model.SortField));
        Assert.True(String.IsNullOrEmpty(model.SortDirection));
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
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var searchFilter = SetDobFilters(1, 0, 2015);
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // act
        var sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.DobFilter(searchViewModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        Assert.True(searchViewModel.FilterErrors.DobErrorNoMonth);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DownloadSelectedFEDataULN_returns_data()
    {
        // arrange
        var upn = _paginatedResultsFake.GetUpn();
        var downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = new string[] { "csv" }
        };

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
        var sut = GetController();

        var result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        // assert
        Assert.IsType<FileContentResult>(result);
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
            Arg.Any<LearnerTextSearchMappingContext>()).Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // act
        FELearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.ToDownloadSelectedFEDataULN(searchViewModel);

        // assert
        Assert.IsType<ViewResult>(result);
        ViewResult? viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel? model = viewResult.Model as LearnerTextSearchViewModel;
        AssertAbstractValues(sut, model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Theory]
    [InlineData(DownloadFileType.None, new string[] { "csv" }, new byte[0], Messages.Search.Errors.SelectFileType)]
    [InlineData(DownloadFileType.CSV, null, new byte[0], Messages.Search.Errors.SelectOneOrMoreDataTypes)]
    [InlineData(DownloadFileType.CSV, new string[] { "csv" }, null, Messages.Downloads.Errors.NoDataForSelectedPupils)]
    public async Task DownloadSelectedFEDataULN_returns_correct_validation_error_message(DownloadFileType downloadFileType, string[] selectedDownloadOptions, byte[] fileBytes, string errorMessage)
    {
        // arrange
        var upn = _paginatedResultsFake.GetUpn();
        var downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = downloadFileType,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = selectedDownloadOptions
        };

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
                Bytes = fileBytes
            });

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // act
        var sut = GetController();
        sut.TempData = tempData;

        var result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerDownloadViewModel;
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);
        Assert.Equal(errorMessage, model.ErrorDetails);
    }

    [Fact]
    public async Task DownloadSelectedFEDataULN_redirects_to_error_when_file_isNull()
    {
        // arrange
        var upn = _paginatedResultsFake.GetUpn();
        var downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = new string[] { "csv" }
        };

        _mockDownloadService.GetFECSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>())
            .Returns(new ReturnFile()
            {
                FileName = null,
                FileType = null,
                Bytes = null
            });

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // act
        var sut = GetController();
        sut.TempData = tempData;

        var result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        var model = viewResult.Model as LearnerDownloadViewModel;
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);
    }
    [Fact]
    public async Task DownloadSelectedFEDataULN_redirects_to_error_when_downloadFile_isNull()
    {
        // arrange
        var upn = _paginatedResultsFake.GetUpn();
        var downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = new string[] { "csv" }
        };

        _mockDownloadService.GetFECSVFile(
            new string[] { "inexistentLearner" },
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>())
            .Returns(new ReturnFile()
            {
                FileName = null,
                FileType = null,
                Bytes = null
            });

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // act
        var sut = GetController();
        sut.TempData = tempData;

        var result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        Assert.IsType<RedirectToActionResult>(result);

    }

    [Fact]
    public async Task DownloadSelectedFEDataULN_redirects_to_error_when_learnerNumber_isNull()
    {
        // arrange
        var upn = _paginatedResultsFake.GetUpn();
        var downloadViewModel = new LearnerDownloadViewModel
        {
            SelectedPupils = upn,
            LearnerNumber = null,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = new string[] { "csv" }
        };

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

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // act
        var sut = GetController();
        sut.TempData = tempData;

        var result = await sut.DownloadFurtherEducationFile(downloadViewModel);

        Assert.IsType<RedirectToActionResult>(result);

    }

    [Fact]
    public async Task Sort_is_cleared_when_filters_are_set()
    {
        // Arrange
        SetupContentServicePublicationSchedule();
        var searchText = "John Smith";
        var surnameFilter = "Surname";
        var searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        // act
        var sut = GetController();

        //_mockSession.SetString(sut.SortDirectionKey, "asc");
        //_mockSession.SetString(sut.SortFieldKey, "Forename");
        _sessionProvider.SetSessionValue(sut.SortDirectionKey, "asc");
        _sessionProvider.SetSessionValue(sut.SortFieldKey, "Forename");

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        var result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        var model = viewResult.Model as LearnerTextSearchViewModel;

        Assert.Null(model.SortField);
        Assert.Null(model.SortDirection);
    }

    private LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(string searchText, SearchFilters searchFilters, string[] selectedGenderValues = null)
    {
        return new LearnerTextSearchViewModel()
        {
            SearchText = searchText,
            SearchFilters = searchFilters,
            SelectedGenderValues = selectedGenderValues,
            Learners = _paginatedResultsFake.GetValidLearners().Learners,
            PageHeading = "Advanced search ULN",
            DownloadLinksPartial = "~/Views/Shared/LearnerText/_SearchFurtherEducationDownloadLinks.cshtml",
            InvalidUPNsConfirmationAction = "",
            LearnerTextSearchController = "FELearnerTextSearch",
            LearnerTextSearchAction = "FurtherEducationNonUlnSearch",
            LearnerNumberController = "search",
            LearnerNumberAction = "pupil-uln"
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

    private void SetupContentServicePublicationSchedule()
    {
        var expectedCommonResponseBody = new CommonResponseBody()
        {
            Id = "PublicationSchedule",
            Title = "Title",
            Body = "Body"
        };
    }

    private void AssertAbstractValues(FELearnerTextSearchController controller, LearnerTextSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
        Assert.Equal(controller.SearchController, model.LearnerTextSearchController);
        Assert.Equal(controller.SearchAction, model.LearnerTextSearchAction);
        Assert.Equal(controller.SearchLearnerNumberController, model.LearnerNumberController);
        Assert.Equal(controller.SearchLearnerNumberAction, model.LearnerNumberAction);
    }


    private FELearnerTextSearchController GetController()
    {
        var user = new UserClaimsPrincipalFake().GetFEApproverClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumULNsPerSearch = 4000,
            DownloadOptionsCheckLimit = 500
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        var httpContextStub = new DefaultHttpContext() { User = user, Session = new Mock<ISession>().Object };
        var mockTempData = new TempDataDictionary(httpContextStub, _mockTempDataProvider);

        List<AvailableDatasetResult> availableDatasetResults = new()
            {
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.PP, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.SEN, HasData: true, CanDownload: true)
            };
        GetAvailableDatasetsForPupilsResponse response = new(availableDatasetResults);

        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(response);

        return new FELearnerTextSearchController(
            _sessionProvider,
            _mockUseCase,
            _mockLearnerSearchResponseToViewModelMapper,
            _mockFiltersRequestMapper,
            _mockSortOrderMapper,
            _mockFiltersRequestBuilder,
            _mockLogger,
            _mockPaginatedService,
            _mockSelectionManager,
            _mockDownloadService,
            _mockAppOptions,
            mockGetAvailableDatasetsForPupilsUseCase.Object)
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
            CurrentFiltersApplied = new List<CurrentFilterDetail>()
                {
                    new CurrentFilterDetail()
                    {
                        FilterType = FilterType.Dob,
                        FilterName = $"{day}/{month}/{year}"
                    }
                },
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
