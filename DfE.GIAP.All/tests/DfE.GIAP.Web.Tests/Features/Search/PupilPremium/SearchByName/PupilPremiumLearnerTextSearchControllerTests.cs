using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.PupilPremium;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Downloads.Services;
using DfE.GIAP.Web.Features.Search.Options;
using DfE.GIAP.Web.Features.Search.PupilPremium.SearchByName;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Tests.Features.Search.PupilPremium.TestDoubles;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;

namespace DfE.GIAP.Web.Tests.Features.Search.PupilPremium.SearchByName;

public sealed class PupilPremiumLearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
{
    private readonly ILogger<PupilPremiumLearnerTextSearchController> _mockLogger = Substitute.For<ILogger<PupilPremiumLearnerTextSearchController>>();
    private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private AzureAppSettings _mockAppSettings = new();
    private readonly SessionFake _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly SearchFiltersFakeData _searchFiltersFake;
    private readonly Mock<ISessionProvider> _mockSessionProvider = new();

    private readonly IUseCase<PupilPremiumSearchRequest, PupilPremiumSearchResponse> _mockUseCase =
        Substitute.For<IUseCase<PupilPremiumSearchRequest, PupilPremiumSearchResponse>>();

    private readonly IMapper<PupilPremiumLearnerTextSearchMappingContext, LearnerTextSearchViewModel> _mockLearnerSearchResponseToViewModelMapper =
        Substitute.For<IMapper<PupilPremiumLearnerTextSearchMappingContext, LearnerTextSearchViewModel>>();

    private readonly IMapper<Dictionary<string, string[]>, IList<FilterRequest>> _mockFiltersRequestMapper =
        Substitute.For<IMapper<Dictionary<string, string[]>, IList<FilterRequest>>>();

    private readonly IFiltersRequestFactory _mockFiltersRequestBuilder = Substitute.For<IFiltersRequestFactory>();

    private readonly IMapper<SortOrderRequest, SortOrder> _mockSortOrderMapper =
        Substitute.For<IMapper<SortOrderRequest, SortOrder>>();

    private readonly Mock<ISearchCriteriaProvider> _mockSearchCriteriaProvider = new();


    public PupilPremiumLearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
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

        _mockSearchCriteriaProvider.Setup(t => t.GetCriteria(It.IsAny<string>())).Returns(SearchCriteriaTestDouble.Stub());

        PupilPremiumSearchResponse response =
            PupilPremiumSearchResponseTestDouble.CreateSuccessResponse();

        _mockUseCase.HandleRequestAsync(
            Arg.Any<PupilPremiumSearchRequest>()).Returns(response);

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = "Somethuiing",
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });
    }

    [Fact]
    public async Task NonUpnPupilPremiumDatabase_returns_empty_page_when_first_navigated_to()
    {
        // Arrange
        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act

        IActionResult result = await sut.NonUpnPupilPremiumDatabase(null);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        _mockSelectionManager.Received().Clear();
        AssertAbstractValues(sut, model);
        Assert.True(string.IsNullOrEmpty(model.SearchText));
    }

    [Fact]
    public async Task NonUpnPupilPremiumDatabase_clears_search_when_return_to_search_is_false()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        PupilPremiumLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        // Act
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(false);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(string.IsNullOrEmpty(model.SearchText));
        Assert.False(
            model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnPupilPremiumDatabase_return_to_search_page_persists_search()
    {
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        const string PupilPremiumSearchTextSessionKey = "SearchPPNonUPN_SearchText";
        const string PupilPremiumSearchFiltersSessionKey = "SearchPPNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(PupilPremiumSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                PupilPremiumSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(true);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(
            model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    //[Fact]
    //public async Task NonUpnPupilPremiumDatabase_does_not_call_GetPage_if_model_state_not_valid()
    //{
    //    // Arrange
    //    PupilPremiumLearnerTextSearchController sut = GetController();

    //    // Act
    //    await sut.NonUpnPupilPremiumDatabase(new LearnerTextSearchViewModel(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    //}

    //[Fact]
    //public async Task NonUpnNationalPupilDatabase_populates_LearnerNumberIds_with_Id_when_UPN_0()
    //{
    //    // Arrange
    //    PupilPremiumLearnerTextSearchController sut = GetController();
    //    //override default user to make admin so Ids are not masked, not testing rbac rules for this test
    //    sut.ControllerContext.HttpContext.User = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();

    //    _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

    //    PaginatedResponse response = new()
    //    {
    //        Learners =
    //        [
    //            new Learner()
    //            {
    //                Id = "123",
    //                LearnerNumber = "0",
    //            },
    //            new Learner()
    //            {
    //                Id = "456",
    //                LearnerNumber = "A203202811068",
    //            }
    //        ],
    //        Count = 2
    //    };
    //    List<Learner> expectedLearners = [
    //        new Learner()
    //        {
    //            Id = "123",
    //            LearnerNumber = "0",
    //            LearnerNumberId = "123",
    //        },
    //        new Learner()
    //        {
    //            Id = "456",
    //            LearnerNumber = "A203202811068",
    //            LearnerNumberId = "A203202811068",
    //        }
    //    ];

    //    // Act
    //    IActionResult result = await sut.NonUpnPupilPremiumDatabase(true);

    //    // Assert
    //    ViewResult viewResult = Assert.IsType<ViewResult>(result);

    //    LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
    //    Assert.True(model.Learners.SequenceEqual(expectedLearners));
    //}

    [Theory]
    [ClassData(typeof(DobSearchFilterTestData))]
    public async Task DobFilter_Adds_DOB_month_and_year_filter_as_expected(SearchFilters searchFilter)
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
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

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

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

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
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

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
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

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
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

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.True(searchViewModel.FilterErrors.DobErrorMonthOnly);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_DobErrorNoMonth()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 0, 2015);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.True(searchViewModel.FilterErrors.DobErrorNoMonth);
        Assert.True(searchViewModel.FilterErrors.DobError);
    }

    [Fact]
    public async Task DobFilter_returns_DobError_when_MonthOutOfRange()
    {
        // Arrange
        string searchText = "John Smith";
        SearchFilters searchFilter = SetDobFilters(1, 99, 2015);
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
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

        PupilPremiumLearnerTextSearchController sut = GetController();

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

        PupilPremiumLearnerTextSearchController sut = GetController();

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
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Surname, searchViewModel.SearchFilters.CustomFilterText.Surname);
    }

    [Fact]
    public async Task MiddlenameFilter_Returns_to_route_with_correct_middlename_filter()
    {
        // Arrange
        string searchText = "John Smith";
        string middlenameFilter = "Middle";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);
        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.MiddlenameFilter(searchViewModel, middlenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Middlename, searchViewModel.SearchFilters.CustomFilterText.Middlename);
    }

    [Fact]
    public async Task ForneameFilter_Returns_to_route_with_correct_forename_filter()
    {
        // Arrange
        string searchText = "John Smith";
        string forenameFilter = "Forename";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText,
                _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.ForenameFilter(searchViewModel, forenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Forename, searchViewModel.SearchFilters.CustomFilterText.Forename);
    }

    [Theory]
    [InlineData("M")]
    [InlineData("F")]
    [InlineData("O")]
    public async Task SexFilter_Returns_to_route_with_correct_gender_filter(string sexFilter)
    {
        // Arrange
        const string searchText = "John Smith";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                SelectedSexValues = [sexFilter],
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        PupilPremiumLearnerTextSearchController sut = GetController();

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
    public async Task SexFilter_returns_all_genders_when_no_sex_selected()
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
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

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
    public async Task SexFilter_returns_all_sex_when_more_than_one_sex_deselected()
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
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedSexValues);
    }

    [Fact]
    public async Task PPAddToMyPupilList_Adds_pupil_to_my_pupil_list_successfully()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpn();

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.True(model.ItemAddedToMyPupilList);
    }

    [Fact]
    public async Task PPAddToMyPupilList_Returns_to_search_page_if_no_pupil_selected()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task PPAddToMyPupilList_sets_download_links_correctly_on_serach_reload()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpn();

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedPupilPremiumDataLink, model.DownloadSelectedLink);
        Assert.Equal(ApplicationLabels.AddSelectedToMyPupilListLink, model.AddSelectedToMyPupilListLink);
    }


    [Fact]
    public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_data_when_pupil_selected()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpn();
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        searchViewModel.PageLearnerNumbers = upn;
        searchViewModel.SelectedPupil = upn;

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DownloadPupilPremiumFile_redirects_to_error_when_there_are_no_files_to_download()
    {
        // arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };
        downloadViewModel.TextSearchViewModel.StarredPupilConfirmationViewModel.ConfirmationGiven = true;

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadPupilPremiumFile(downloadViewModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DownloadFileConfirmationReturn_redirects_to_ConfirmationForStarredPupil_when_no_confirmation_given()
    {
        // arrange
        StarredPupilConfirmationViewModel StarredPupilConfirmationViewModel = new()
        {
            SelectedPupil = _paginatedResultsFake.GetUpn(),
            DownloadType = Common.Enums.DownloadType.PupilPremium,
            ConfirmationGiven = false,
            ConfirmationError = true,
            ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction,
            CancelReturnAction = Global.PPDownloadCancellationReturnAction,
            LearnerNumbers = _paginatedResultsFake.GetUpn()
        };

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel);

        // assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DownloadPupilPremiumFile_redirects_to_error_when_no_data_available()
    {
        // arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true
        };

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadPupilPremiumFile(downloadViewModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        string upn = string.Empty;
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

        // assert        
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_starred_pupil_confirmation_if_starred_pupil_selected()
    {
        // arrange
        string upn = _paginatedResultsFake.GetBase64EncodedUpn();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(upn, starredPupilViewModel.SelectedPupil);
    }

    [Theory]
    [InlineData(DownloadFileType.None, new byte[0])]
    [InlineData(DownloadFileType.CSV, new byte[0])]
    [InlineData(DownloadFileType.CSV, null)]
    public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_correct_validation_error_message(
        DownloadFileType downloadFileType,
        byte[]? fileBytes)
    {
        // arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            SelectedPupilsCount = 1,
            DownloadFileType = downloadFileType,
            ShowTABDownloadType = true
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());


        PupilPremiumLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        // act
        IActionResult result = await sut.DownloadPupilPremiumFile(downloadViewModel);

        // arrange
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DownloadCancellationReturn_redirects_to_search()
    {
        // arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        const string PupilPremiumSearchTextSessionKey = "SearchPPNonUPN_SearchText";
        const string PupilPremiumSearchFiltersSessionKey = "SearchPPNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(PupilPremiumSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                PupilPremiumSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadCancellationReturn();

        // assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task DownloadCancellationReturn_redirects_to_search_sets_download_links()
    {
        // arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        const string PupilPremiumSearchTextSessionKey = "SearchPPNonUPN_SearchText";
        const string PupilPremiumSearchFiltersSessionKey = "SearchPPNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(PupilPremiumSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                PupilPremiumSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        PupilPremiumLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadCancellationReturn();

        // assert            
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedPupilPremiumDataLink, model.DownloadSelectedLink);
        Assert.Equal(ApplicationLabels.AddSelectedToMyPupilListLink, model.AddSelectedToMyPupilListLink);
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
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        PupilPremiumLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        // act
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, sortField, sortDirection);

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
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        string sortField = "Forename";
        string sortDirection = "asc";


        PupilPremiumLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        // act
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

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

        string sortField = "Forename";
        string sortDirection = "asc";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        searchViewModel.SortField = sortField;
        searchViewModel.SortDirection = sortDirection;

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        PupilPremiumLearnerTextSearchController sut = GetController();

        const string PupilPremiumSearchTextSessionKey = "SearchPPNonUPN_SearchText";
        const string PupilPremiumSearchFiltersSessionKey = "SearchPPNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(PupilPremiumSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(PupilPremiumSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                PupilPremiumSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        // act
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(true);

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<PupilPremiumLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners

            });

        string ? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;
        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        PupilPremiumLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        IActionResult result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

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
    public async Task Sort_is_cleared_when_filters_are_set()
    {
        // Arrange
        string searchText = "John Smith";
        string surnameFilter = "Surname";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        PupilPremiumLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        // act
        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Null(model.SortField);
        Assert.Null(model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_cleared_when_filters_are_removed()
    {
        // Arrange
        const string searchText = "John Smith";
        const string surnameFilter = "";
        const string? middlenameFilter = null;
        const string? forenameFilter = null;
        const string searchByRemove = "Male";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText, _searchFiltersFake.GetSearchFilters());

        ITempDataDictionary mockTempDataDictionary = Substitute.For<ITempDataDictionary>();
        mockTempDataDictionary.Add("PersistedSelectedGenderFilters", searchByRemove);
        PupilPremiumLearnerTextSearchController sut = GetController();
        sut.TempData = mockTempDataDictionary;

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        // act
        IActionResult result =
            await sut.NonUpnPupilPremiumDatabase(
                searchViewModel,
                surnameFilter,
                middlenameFilter,
                forenameFilter,
                searchByRemove,
                string.Empty,
                string.Empty);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.NotNull(model);
        Assert.True(string.IsNullOrEmpty(model.SortField));
        Assert.True(string.IsNullOrEmpty(model.SortDirection));
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
            PageHeading = "Advanced search pupil premium",
            DownloadLinksPartial = "~/Views/Shared/LearnerText/_SearchPupilPremiumDownloadLinks.cshtml",
            LearnerTextSearchController = "PupilPremiumLearnerTextSearch",
            LearnerTextSearchAction = "NonUpnPupilPremiumDatabase",
            LearnerNumberController = "search",
            LearnerNumberAction = "pupil-premium"
        };
    }


    private static void AssertAbstractValues(PupilPremiumLearnerTextSearchController controller, LearnerTextSearchViewModel model)
    {
        Assert.Equal(ApplicationLabels.SearchPupilPremiumWithOutUpnPageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.SearchController, model.LearnerTextSearchController);
        Assert.Equal(controller.SearchAction, model.LearnerTextSearchAction);
        Assert.Equal(Routes.Application.Search, model.LearnerNumberController);
        Assert.Equal(controller.SearchLearnerNumberAction, model.LearnerNumberAction);
    }


    private PupilPremiumLearnerTextSearchController GetController()
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumUPNsPerSearch = 4000,
            DownloadOptionsCheckLimit = 500,
            MaximumNonUPNResults = 100
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        DefaultHttpContext httpContextStub = new()
        {
            User = user,
            Session = _mockSession
        };

        TempDataDictionary mockTempData = new(httpContextStub, Substitute.For<ITempDataProvider>());

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


        return new PupilPremiumLearnerTextSearchController(
            _mockLogger,
            _mockAppOptions,
            _mockSelectionManager,
            _mockSessionProvider.Object,
            new Mock<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>().Object,
            downloadPupilPremiumDataServiceMock.Object,
            _mockUseCase,
            _mockLearnerSearchResponseToViewModelMapper,
            _mockFiltersRequestMapper,
            _mockSortOrderMapper,
            _mockFiltersRequestBuilder,
            _mockSearchCriteriaProvider.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextStub
            },
            TempData = mockTempData
        };
    }

    private static SearchFilters SetDobFilters(int day, int month, int year)
    {
        return new SearchFilters()
        {
            CurrentFiltersApplied = [
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

    /*https://bytelanguage.net/2020/07/31/writing-unit-test-for-model-validation/*/

    private static void MockModelState<TModel, TController>(TModel model, TController controller) where TController : ControllerBase
    {
        ValidationContext validationContext = new(model!, null, null);
        List<ValidationResult> validationResults = [];
        Validator.TryValidateObject(model!, validationContext, validationResults, true);
        foreach (ValidationResult validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage!);
        }
    }
}
