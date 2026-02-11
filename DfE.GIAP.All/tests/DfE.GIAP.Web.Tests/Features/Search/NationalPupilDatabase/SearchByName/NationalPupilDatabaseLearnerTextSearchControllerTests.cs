using System.Security.Claims;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.CrossCutting.Logging.Events;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf;
using DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilDatasets;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Core.Search.Application.Models.Filter;
using DfE.GIAP.Core.Search.Application.Models.Search;
using DfE.GIAP.Core.Search.Application.Models.Sort;
using DfE.GIAP.Core.Search.Application.Options.Search;
using DfE.GIAP.Core.Search.Application.Options.Sort;
using DfE.GIAP.Core.Search.Application.UseCases.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.SharedTests.TestDoubles;
using DfE.GIAP.Web.Features.Search.NationalPupilDatabase.SearchByName;
using DfE.GIAP.Web.Features.Search.Shared.Filters;
using DfE.GIAP.Web.Features.Search.Shared.Sort;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
using DfE.GIAP.Web.Services.Download.CTF;
using DfE.GIAP.Web.Tests.TestDoubles;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NSubstitute;

namespace DfE.GIAP.Web.Tests.Features.Search.NationalPupilDatabase.SearchByName;

public sealed class NationalPupilDatabaseLearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
{
    private readonly ILogger<NationalPupilDatabaseLearnerTextSearchController> _mockLogger = Substitute.For<ILogger<NationalPupilDatabaseLearnerTextSearchController>>();
    private readonly IDownloadCommonTransferFileService _mockCtfService = Substitute.For<IDownloadCommonTransferFileService>();
    private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
    private readonly Mock<ISessionProvider> _mockSessionProvider = new();
    private readonly SessionFake _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly SearchFiltersFakeData _searchFiltersFake;


    private readonly IUseCase<NationalPupilDatabaseSearchByNameRequest, NationalPupilDatabaseSearchByNameResponse> _mockUseCase =
        Substitute.For<IUseCase<NationalPupilDatabaseSearchByNameRequest, NationalPupilDatabaseSearchByNameResponse>>();

    private readonly IMapper<NationalPupilDatabaseLearnerTextSearchMappingContext, LearnerTextSearchViewModel> _mockLearnerSearchResponseToViewModelMapper =
        Substitute.For<IMapper<NationalPupilDatabaseLearnerTextSearchMappingContext, LearnerTextSearchViewModel>>();

    private readonly IMapper<Dictionary<string, string[]>, IList<FilterRequest>> _mockFiltersRequestMapper =
        Substitute.For<IMapper<Dictionary<string, string[]>, IList<FilterRequest>>>();

    private readonly IFiltersRequestFactory _mockFiltersRequestFactory = Substitute.For<IFiltersRequestFactory>();

    private readonly Mock<ISearchIndexOptionsProvider> _searchindexOptionsProvider = new();

    private readonly Mock<ISortOrderFactory> _sortOrderFactoryMock = new();

    private readonly Mock<IMapper<SearchCriteriaOptions, SearchCriteria>> _criteriaOptionsToCriteriaMock = new();

    public NationalPupilDatabaseLearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
        _searchFiltersFake = searchFiltersFake;

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


        NationalPupilDatabaseSearchByNameResponse response =
            NationalPupilDatabaseSearchByNameResponseTestDoubles.CreateSuccessResponse();

        _mockUseCase.HandleRequestAsync(
            Arg.Any<NationalPupilDatabaseSearchByNameRequest>()).Returns(response);

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = "Somethuiing",
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_returns_empty_page_when_first_navigated_to()
    {
        // Arrange
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(null);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        _mockSelectionManager.Received().Clear();
        AssertAbstractValues(sut, model);
        Assert.True(string.IsNullOrEmpty(model.SearchText));
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_clears_search_when_return_to_search_is_false()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        IActionResult result = await sut.NonUpnNationalPupilDatabase(false);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(string.IsNullOrEmpty(model.SearchText));
        Assert.False(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_return_to_search_page_persists_search()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        const string NpdSearchTextSessionKey = "SearchNonUPN_SearchText";
        const string NpdSearchFiltersSessionKey = "SearchNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(NpdSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(NpdSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(NpdSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                NpdSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(true);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_search_returns_results()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(
            searchViewModel,
            surnameFilter,
            middlenameFilter,
            forenameFilter,
            null,
            null,
            searchByRemove);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    //[Fact]
    //public async Task NonUpnNationalPupilDatabase_does_not_call_GetPage_if_model_state_not_valid()
    //{
    //    // Arrange
    //    NationalPupilDatabaseLearnerTextSearchController sut = GetController();

    //    // Act
    //    await sut.NonUpnNationalPupilDatabase(new LearnerTextSearchViewModel(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

    //    // Assert
    //}

    //[Fact]
    //public async Task NonUpnNationalPupilDatabase_populates_LearnerNumberIds_with_Id_when_UPN_0()
    //{
    //    // Arrange
    //    NationalPupilDatabaseLearnerTextSearchController sut = GetController();
    //    //override default user to make admin so Ids are not masked, not testing rbac rules for this test
    //    sut.ControllerContext.HttpContext.User = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();


    //    _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
    //    PaginatedResponse response = new()
    //    {
    //        Learners = [
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
    //    IActionResult result = await sut.NonUpnNationalPupilDatabase(true);

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
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
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
        const string searchText = "John Smith";
        const string surnameFilter = "Surname";

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();
        const string sortField = "Forename";
        const string sortDirection = "asc";

        LearnerTextSearchViewModel searchViewModel = new()
        {
            SearchText = searchText,
            SearchFilters = searchFilters,
            Learners = _paginatedResultsFake.GetValidLearners().Learners,
            SortField = sortField,
            SortDirection = sortDirection
        };

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
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
    public async Task MiddlenameFilter_Returns_to_route_with_correct_middlename_filter()
    {
        // Arrange
        string searchText = "John Smith";
        string middlenameFilter = "Middle";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.MiddlenameFilter(searchViewModel, middlenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SearchFilters.CustomFilterText.Middlename, searchViewModel.SearchFilters.CustomFilterText.Middlename);
    }

    [Fact]
    public async Task ForneameFilter_Returns_to_route_with_correct_forename_filter()
    {
        string searchText = "John Smith";
        string forenameFilter = "Forename";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.ForenameFilter(searchViewModel, forenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
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
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                SelectedSexValues = [sexFilter],
                Learners = _paginatedResultsFake.GetValidLearners().Learners
            });

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SelectedSexValues[0], sexFilter);
    }


    [Fact]
    public async Task SexFilter_returns_all_genders_when_no_gender_selected()
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
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedSexValues);
    }

    [Fact]
    public async Task SexFilter_returns_all_genders_when_more_than_one_gender_deselected()
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
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedSexValues);
    }



    [Fact]
    public async Task NonUpnAddToMyPupilList_Adds_pupil_to_my_pupil_list_successfully()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpn();

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.NonUpnAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.True(model.ItemAddedToMyPupilList);
    }

    [Fact]
    public async Task NonUpnAddToMyPupilList_Returns_to_search_page_if_no_pupil_selected()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.NonUpnAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_options_page_when_pupils_selected()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetUpn();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // Act
        IActionResult result = await sut.ToDownloadSelectedNPDDataNonUPN(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        Assert.Equal(model.SelectedPupils, upn);
        Assert.Equal(1, model.SelectedPupilsCount);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // Arrange
        string upn = string.Empty;
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        sut.TempData = Substitute.For<ITempDataDictionary>();

        // Act
        IActionResult result = await sut.ToDownloadSelectedNPDDataNonUPN(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task ToDownloadSelectedNPDDataNonUPN_returns_starred_pupil_confirmation_if_starred_pupil_selected()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetBase64EncodedUpn();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.ToDownloadSelectedNPDDataNonUPN(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(Common.Enums.DownloadType.NPD, starredPupilViewModel.DownloadType);
        Assert.Equal(upn, starredPupilViewModel.SelectedPupil);

    }

    [Theory]
    [InlineData("A203102209083")]
    [InlineData("QTIwMzEwMjIwOTA4Mw==-GIAP")]
    public async Task DownloadNpdCommonTransferFileData_returns_data(string upn)
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        searchViewModel.StarredPupilConfirmationViewModel = new StarredPupilConfirmationViewModel()
        {
            ConfirmationGiven = true
        };
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        _mockCtfService.GetCommonTransferFile(
        Arg.Any<string[]>(),
        Arg.Any<string[]>(),
        Arg.Any<string>(),
        Arg.Any<string>(),
        Arg.Any<bool>(),
        Arg.Any<AzureFunctionHeaderDetails>(),
        Arg.Any<ReturnRoute>()
        ).Returns(new ReturnFile()
        {
            FileName = "test",
            FileType = FileType.ZipFile,
            Bytes = []
        });

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadNpdCommonTransferFileData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // Arrange
        string upn = string.Empty;
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }


    [Fact]
    public async Task DownloadNpdCommonTransferFileData_returns_starred_pupil_confirmation_if_starred_pupil_selected()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetBase64EncodedUpn();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(Common.Enums.DownloadType.CTF, starredPupilViewModel.DownloadType);
        Assert.Equal(upn, starredPupilViewModel.SelectedPupil);
    }

    [Fact]
    public async Task DownloadNpdCommonTransferFileData_returns_to_search_page_if_download_null()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetUpn();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        _mockCtfService.GetCommonTransferFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>()
            ).Returns(new ReturnFile()
            {
                FileName = "test",
                FileType = FileType.ZipFile,
                Bytes = null
            });

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
    }

    [Fact]
    public async Task DownloadFileConfirmationReturn_returns_starred_pupil_confirmation_if_confirmation_not_provided()
    {
        // Arrange
        StarredPupilConfirmationViewModel starredPupilConfirmationViewModel = new()
        {
            SelectedPupil = _paginatedResultsFake.GetBase64EncodedUpn(),
            ConfirmationGiven = false
        };

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act

        IActionResult result = await sut.DownloadFileConfirmationReturn(starredPupilConfirmationViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(_paginatedResultsFake.GetBase64EncodedUpn(), starredPupilViewModel.SelectedPupil);
    }

    [Fact]
    public async Task DownloadFileConfirmationReturn_returns_CTF_file_if_confirmation_provided()
    {
        // Arrange
        StarredPupilConfirmationViewModel starredPupilConfirmationViewModel = new()
        {
            SelectedPupil = _paginatedResultsFake.GetBase64EncodedUpn(),
            ConfirmationGiven = true,
            DownloadType = Common.Enums.DownloadType.CTF
        };

        _mockCtfService.GetCommonTransferFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>()
            ).Returns(new ReturnFile()
            {
                FileName = "test",
                FileType = FileType.ZipFile,
                Bytes = []
            });

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.DownloadFileConfirmationReturn(starredPupilConfirmationViewModel);

        // Assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadFileConfirmationReturn_returns_NPD_file_if_confirmation_provided()
    {
        // Arrange
        StarredPupilConfirmationViewModel starredPupilConfirmationViewModel = new()
        {
            SelectedPupil = _paginatedResultsFake.GetBase64EncodedUpn(),
            ConfirmationGiven = true,
            DownloadType = Common.Enums.DownloadType.NPD
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        // Act
        IActionResult result = await sut.DownloadFileConfirmationReturn(starredPupilConfirmationViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);

        LearnerDownloadViewModel model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        Assert.Equal(_paginatedResultsFake.GetBase64EncodedUpn(), model.SelectedPupils);
        Assert.Equal(1, model.SelectedPupilsCount);
    }

    [Fact]
    public async Task DownloadCancellationReturn_returns_to_search()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        const string NpdSearchTextSessionKey = "SearchNonUPN_SearchText";
        const string NpdSearchFiltersSessionKey = "SearchNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(NpdSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(NpdSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(NpdSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                NpdSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        // Act
        IActionResult result = await sut.DownloadCancellationReturn();

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Theory]
    [InlineData("Forename", "asc")]
    [InlineData("Surname", "desc")]
    public async Task Sort_is_correctly_handled(string sortField, string sortDirection)
    {
        // Arrange
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
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, sortField, sortDirection);

        // Assert
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
        // Arrange
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
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>())
                .Returns(searchViewModel);

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        string sortField = "Forename";
        string sortDirection = "asc";

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // Assert
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
        // Arrange
        string searchText = "John Smith";

        string sortField = "Forename";
        string sortDirection = "asc";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        searchViewModel.SortField = sortField;
        searchViewModel.SortDirection = sortDirection;

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(searchViewModel);

        const string NpdSearchTextSessionKey = "SearchNonUPN_SearchText";
        const string NpdSearchFiltersSessionKey = "SearchNonUPN_SearchFilters";

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(NpdSearchTextSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.ContainsSessionKey(NpdSearchFiltersSessionKey)).Returns(true).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValue(NpdSearchTextSessionKey)).Returns(searchText).Verifiable();

        _mockSessionProvider.Setup(
            (t) => t.GetSessionValueOrDefault<SearchFilters>(
                NpdSearchFiltersSessionKey)).Returns(
                    searchViewModel.SearchFilters).Verifiable();

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(true);

        // Assert
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
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        SearchFilters searchFilters = _searchFiltersFake.GetSearchFilters();

        _mockLearnerSearchResponseToViewModelMapper.Map(
            Arg.Any<NationalPupilDatabaseLearnerTextSearchMappingContext>()).Returns(
            new LearnerTextSearchViewModel()
            {
                SearchText = searchText,
                SearchFilters = searchFilters,
                Learners = _paginatedResultsFake.GetValidLearners().Learners

            });

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;
        string sortField = "Forename";
        string sortDirection = "asc";

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // Assert
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

        NationalPupilDatabaseLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        // Act
        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
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
        NationalPupilDatabaseLearnerTextSearchController sut = GetController();
        sut.TempData = mockTempDataDictionary;

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        // Act
        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, "", "");

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(string.IsNullOrEmpty(model.SortField));
        Assert.True(string.IsNullOrEmpty(model.SortDirection));
    }

    private LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(
        string searchText,
        SearchFilters searchFilters,
        string[]? selectedSexValues = null)
    {
        return new()
        {
            SearchText = searchText,
            SearchFilters = searchFilters,
            SelectedSexValues = selectedSexValues,
            Learners = _paginatedResultsFake.GetValidLearners().Learners,
            PageHeading = "Advanced search NPD",
            DownloadLinksPartial = "~/Views/Shared/LearnerText/_SearchPageDownloadLinks.cshtml",
            LearnerTextSearchController = "NationalPupilDatabaseLearnerTextSearch",
            LearnerTextSearchAction = "NonUpnNationalPupilDatabase",
            LearnerNumberController = "search",
            LearnerNumberAction = "national-pupil-database"
        };
    }

    private static void AssertAbstractValues(NationalPupilDatabaseLearnerTextSearchController controller, LearnerTextSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.SearchController, model.LearnerTextSearchController);
        Assert.Equal(controller.SearchAction, model.LearnerTextSearchAction);
        Assert.Equal(controller.SearchLearnerNumberController, model.LearnerNumberController);
        Assert.Equal(controller.SearchLearnerNumberAction, model.LearnerNumberAction);
    }

    private NationalPupilDatabaseLearnerTextSearchController GetController()
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();

        DefaultHttpContext httpContextStub = new() { User = user, Session = _mockSession };
        TempDataDictionary mockTempData = new(httpContextStub, Substitute.For<ITempDataProvider>());

        List<AvailableDatasetResult> availableDatasetResults = [
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.KS1, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.KS2, HasData: true, CanDownload: true)
            ];
        GetAvailableDatasetsForPupilsResponse response = new(availableDatasetResults);

        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(response);

        Mock<IUseCase<DownloadPupilDataRequest, DownloadPupilDataResponse>> mockDownloadPupilDataUseCase = new();
        mockDownloadPupilDataUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<DownloadPupilDataRequest>()))
            .ReturnsAsync(It.IsAny<DownloadPupilDataResponse>);
        Mock<IEventLogger> mockEventLogger = new();

        return new NationalPupilDatabaseLearnerTextSearchController(
             _mockLogger,
             _mockSelectionManager,
             _mockCtfService,
             _mockSessionProvider.Object,
             mockGetAvailableDatasetsForPupilsUseCase.Object,
             new Mock<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>().Object,
             mockDownloadPupilDataUseCase.Object,
             mockEventLogger.Object,
             _mockUseCase,
             _mockLearnerSearchResponseToViewModelMapper,
             _mockFiltersRequestMapper,
             _sortOrderFactoryMock.Object,
             _mockFiltersRequestFactory,
             _searchindexOptionsProvider.Object,
             _criteriaOptionsToCriteriaMock.Object)

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
}
