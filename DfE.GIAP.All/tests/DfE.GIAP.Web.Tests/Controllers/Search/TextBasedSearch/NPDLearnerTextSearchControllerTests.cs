using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Downloads.Application.UseCases.GetAvailableDatasetsForPupils;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers.TextBasedSearch;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Providers.Session;
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

namespace DfE.GIAP.Web.Tests.Controllers.Search.TextBasedSearch;

public class NPDLearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
{
    private readonly ILogger<NPDLearnerTextSearchController> _mockLogger = Substitute.For<ILogger<NPDLearnerTextSearchController>>();
    private readonly IDownloadCommonTransferFileService _mockCtfService = Substitute.For<IDownloadCommonTransferFileService>();
    private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
    private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
    private readonly IMyPupilListService _mockMplService = Substitute.For<IMyPupilListService>();
    private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private AzureAppSettings _mockAppSettings = new();
    private readonly Mock<ISessionProvider> _mockSessionProvider = new();
    private readonly TestSession _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly SearchFiltersFakeData _searchFiltersFake;

    public NPDLearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
        _searchFiltersFake = searchFiltersFake;
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_returns_empty_page_when_first_navigated_to()
    {
        // Act
        NPDLearnerTextSearchController sut = GetController();
        IActionResult result = await sut.NonUpnNationalPupilDatabase(null);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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
        NPDLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(false);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);

        Assert.True(string.IsNullOrEmpty(model.SearchText));
        Assert.False(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_return_to_search_page_persists_search()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

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
        // Act


        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(true);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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
        string surnameFilter = null;
        string middlenameFilter = null;
        string forenameFilter = null;
        string searchByRemove = null;

        // Act
        NPDLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, null, null, searchByRemove);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnNationalPupilDatabase_does_not_call_GetPage_if_model_state_not_valid()
    {
        // Arrange

        // Act
        NPDLearnerTextSearchController sut = GetController();

        await sut.NonUpnNationalPupilDatabase(new LearnerTextSearchViewModel(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        // Assert
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

    [Fact]
    public async Task NonUpnNationalPupilDatabase_populates_LearnerNumberIds_with_Id_when_UPN_0()
    {
        // Arrange
        NPDLearnerTextSearchController sut = GetController();
        //override default user to make admin so Ids are not masked, not testing rbac rules for this test
        sut.ControllerContext.HttpContext.User = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();


        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
        PaginatedResponse response = new()
        {
            Learners = [
                new Learner()
                {
                    Id = "123",
                    LearnerNumber = "0",
                },
                new Learner()
                {
                    Id = "456",
                    LearnerNumber = "A203202811068",
                }
            ],
            Count = 2
        };
        List<Learner> expectedLearners = [
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
        ];

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, response);

        // Act

        IActionResult result = await sut.NonUpnNationalPupilDatabase(true);

        // Assert

        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);

        Assert.True(model.Learners.SequenceEqual(expectedLearners));
    }


    [Theory]
    [ClassData(typeof(DobSearchFilterTestData))]
    public async Task DobFilter_Adds_DOB_month_and_year_filter_as_expected(SearchFilters searchFilter)
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, searchFilter);

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.DobFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.MiddlenameFilter(searchViewModel, middlenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.ForenameFilter(searchViewModel, forenameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
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
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters(), null, [sexFilter]);

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SelectedSexValues[0], sexFilter);
    }


    [Fact]
    public async Task SexFilter_returns_all_genders_when_no_gender_selected()
    {
        // Arrange
        string searchText = "Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters(), null);
        searchViewModel.SearchFilters.CurrentFiltersAppliedString = @"[{ ""FilterName"":""Female"",""FilterType"":6}]";

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Null(model.SelectedSexValues);
    }

    [Fact]
    public async Task SexFilter_returns_all_genders_when_more_than_one_gender_deselected()
    {
        // Arrange
        string searchText = "Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters(), null);
        searchViewModel.SearchFilters.CurrentFiltersAppliedString = @"[{""FilterName"":""Female"",""FilterType"":6}, {""FilterName"":""Male"",""FilterType"":6}]";

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);

        await _mockMplService.Received().UpdateMyPupilList(
            Arg.Any<IEnumerable<MyPupilListItem>>(),
            Arg.Any<string>(),
            Arg.Any<AzureFunctionHeaderDetails>()
            );
        Assert.True(model.ItemAddedToMyPupilList);
    }

    [Fact]
    public async Task NonUpnAddToMyPupilList_Returns_to_search_page_if_no_pupil_selected()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.AddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task NonUpnAddToMyPupilList_redirects_to_InvalidUPNs_if_invalid_upn_selected()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        IActionResult result = await sut.AddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
    }


    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_options_page_when_pupils_selected()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetUpn();
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();
        IActionResult result = await sut.ToDownloadSelectedNPDDataNonUPN(searchViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        LearnerDownloadViewModel model = viewResult.Model as LearnerDownloadViewModel;

        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);
        Assert.Equal(model.SelectedPupils, upn);
        Assert.Equal(1, model.SelectedPupilsCount);
    }

    [Theory]
    [InlineData("A203102209083")]
    [InlineData("QTIwMzEwMjIwOTA4Mw==-GIAP")]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_data(string upn)
    {
        // Arrange
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = ["csv"]
        };

        _mockDownloadService.GetCSVFile(
           Arg.Any<string[]>(),
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

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // Act
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(downloadViewModel);

        // Assert
        Assert.IsType<FileContentResult>(result);
    }


    [Theory]
#pragma warning disable CA1861 // Avoid constant arrays as arguments
    [InlineData(DownloadFileType.None, new string[] { "csv" }, new byte[0], Messages.Search.Errors.SelectFileType)]
#pragma warning restore CA1861 // Avoid constant arrays as arguments
    [InlineData(DownloadFileType.CSV, null, new byte[0], Messages.Search.Errors.SelectOneOrMoreDataTypes)]
#pragma warning disable CA1861 // Avoid constant arrays as arguments
    [InlineData(DownloadFileType.CSV, new string[] { "csv" }, null, Messages.Downloads.Errors.NoDataForSelectedPupils)]
#pragma warning restore CA1861 // Avoid constant arrays as arguments
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_correct_validation_error_message(DownloadFileType downloadFileType, string[] selectedDownloadOptions, byte[] fileBytes, string errorMessage)
    {
        // Arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = downloadFileType,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = selectedDownloadOptions
        };

        _mockDownloadService.GetCSVFile(
           Arg.Any<string[]>(),
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
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // Act
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(downloadViewModel);

        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        LearnerDownloadViewModel model = viewResult.Model as LearnerDownloadViewModel;
        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);
        Assert.Equal(errorMessage, model.ErrorDetails);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_shows_error_when_file_is_empty()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = ["csv"]
        };

        _mockDownloadService.GetCSVFile(
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           Arg.Any<bool>(),
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>())
           .Returns(new ReturnFile()
           {
               FileName = "test",
               FileType = "csv",
               Bytes = null
           });

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // Act
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(downloadViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_shows_error_when_there_are_no_files_to_download()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetUpn();
        LearnerDownloadViewModel downloadViewModel = new()
        {
            SelectedPupils = upn,
            LearnerNumber = upn,
            ErrorDetails = string.Empty,
            SelectedPupilsCount = 1,
            DownloadFileType = DownloadFileType.CSV,
            ShowTABDownloadType = true,
            SelectedDownloadOptions = ["csv"]
        };

        _mockDownloadService.GetCSVFile(
           Arg.Any<string[]>(),
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
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // Act
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        IActionResult result = await sut.DownloadSelectedNationalPupilDatabaseData(downloadViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // Arrange
        string upn = string.Empty;
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NPDLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        sut.TempData = Substitute.For<ITempDataDictionary>();
        IActionResult result = await sut.ToDownloadSelectedNPDDataNonUPN(searchViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        AssertAbstractValues(sut, model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
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
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.ToDownloadSelectedNPDDataNonUPN(searchViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.Equal(DownloadType.NPD, starredPupilViewModel.DownloadType);
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
        NPDLearnerTextSearchController sut = GetController();

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
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        AssertAbstractValues(sut, model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
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
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.Equal(DownloadType.CTF, starredPupilViewModel.DownloadType);
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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.ToDownloadNpdCommonTransferFileData(searchViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        AssertAbstractValues(sut, model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.DownloadFileConfirmationReturn(starredPupilConfirmationViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;
        StarredPupilConfirmationViewModel starredPupilViewModel = model.StarredPupilConfirmationViewModel;
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
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
            DownloadType = DownloadType.CTF
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

        // Act
        NPDLearnerTextSearchController sut = GetController();

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
            DownloadType = DownloadType.NPD
        };

        _mockDownloadService.GetCSVFile(
           Arg.Any<string[]>(),
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

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        // Act
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = tempData;

        IActionResult result = await sut.DownloadFileConfirmationReturn(starredPupilConfirmationViewModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
        LearnerDownloadViewModel model = viewResult.Model as LearnerDownloadViewModel;

        Assert.Equal(Global.NonLearnerNumberDownloadOptionsView, viewResult.ViewName);
        Assert.Equal(_paginatedResultsFake.GetBase64EncodedUpn(), model.SelectedPupils);
        Assert.Equal(1, model.SelectedPupilsCount);
    }

    [Fact]
    public async Task DownloadCancellationReturn_returns_to_search()
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.DownloadCancellationReturn(new StarredPupilConfirmationViewModel());

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

        AssertAbstractValues(sut, model);
        Assert.Equal(searchText, model.SearchText);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnInvalidUPNs_returns_invalid_upn_page_upns_only()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn
        };

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        IActionResult result = await sut.NonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        InvalidLearnerNumberSearchViewModel vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        Assert.True(vm.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnInvalidUPNs_returns_invalid_upn_page_ids_and_upns()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn
        };

        IEnumerable<Learner> expectedLearners = _paginatedResultsFake.GetInvalidLearners().Learners.Concat(_paginatedResultsFake.GetValidLearners().Learners);

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        InvalidLearnerNumberSearchViewModel vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        Assert.True(vm.Learners.SequenceEqual(expectedLearners));
    }

    [Fact]
    public async Task NonUpnInvalidUPNs_returns_invalid_upn_page_ids_only()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn
        };

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, new PaginatedResponse());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetInvalidLearners());

        IActionResult result = await sut.NonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        InvalidLearnerNumberSearchViewModel vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        Assert.True(vm.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
    }

    [Fact]
    public async Task NonUpnInvalidUPNsConfirmation_redirects_to_my_pupil_list()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn,
            SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_MyPupilList
        };

        // Act
        NPDLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.NonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal(Global.MyPupilListControllerName, viewResult.ControllerName);
        Assert.Equal(Global.MyPupilListAction, viewResult.ActionName);
    }

    [Fact]
    public async Task NonUpnInvalidUPNsConfirmation_redirects_to_search()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn,
            SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_ReturnToSearch
        };

        // Act
        NPDLearnerTextSearchController sut = GetController();

        IActionResult result = await sut.NonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal(Global.NPDNonUpnAction, viewResult.ActionName);
    }

    [Fact]
    public async Task NonUpnInvalidUPNsConfirmation_returns_no_option_selected_validation_message()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn,
            SelectedInvalidUPNOption = string.Empty
        };

        // Act
        NPDLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        MockModelState(invalidLearnerNumberSearchViewModel, sut);

        IActionResult result = await sut.NonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        Assert.Single(sut.ViewData.ModelState["NoContinueSelection"].Errors);
    }

    [Theory]
    [InlineData("Forename", "asc")]
    [InlineData("Surname", "desc")]
    public async Task Sort_is_correctly_handled(string sortField, string sortDirection)
    {
        // Arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        string surnameFilter = null;
        string middlenameFilter = null;
        string forenameFilter = null;
        string searchByRemove = null;

        // Act
        NPDLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, sortField, sortDirection);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        string surnameFilter = null;
        string middlenameFilter = null;
        string forenameFilter = null;
        string searchByRemove = null;

        string sortField = "Forename";
        string sortDirection = "asc";

        // Act
        NPDLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        string sortField = "Forename";
        string sortDirection = "asc";

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(true);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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
        string surnameFilter = null;
        string middlenameFilter = null;
        string forenameFilter = null;
        string searchByRemove = null;
        string sortField = "Forename";
        string sortDirection = "asc";

        // Act
        NPDLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, null, null);

        // Assert
        Assert.IsType<ViewResult>(result);
        ViewResult viewResult = result as ViewResult;

        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);

        Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

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

        // Act
        NPDLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.SurnameFilter(searchViewModel, surnameFilter);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

        Assert.Null(model.SortField);
        Assert.Null(model.SortDirection);
    }

    [Fact]
    public async Task Sort_is_cleared_when_filters_are_removed()
    {
        // Arrange
        const string searchText = "John Smith";
        const string surnameFilter = "";
        const string middlenameFilter = null;
        const string forenameFilter = null;
        const string searchByRemove = "Male";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText, _searchFiltersFake.GetSearchFilters(), selectedGenderValues: ["M"]);

        ITempDataDictionary mockTempDataDictionary = Substitute.For<ITempDataDictionary>();
        mockTempDataDictionary.Add("PersistedSelectedGenderFilters", searchByRemove);
        NPDLearnerTextSearchController sut = GetController();
        sut.TempData = mockTempDataDictionary;

        // Act
        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        IActionResult result = await sut.NonUpnNationalPupilDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, "", "");

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);
        LearnerTextSearchViewModel model = viewResult.Model as LearnerTextSearchViewModel;

        Assert.True(string.IsNullOrEmpty(model.SortField));
        Assert.True(string.IsNullOrEmpty(model.SortDirection));
    }

    private static LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(
        string searchText,
        SearchFilters searchFilters,
        string[] selectedGenderValues = null,
        string[] selectedSexValues = null)
    {
        return new()
        {
            SearchText = searchText,
            SearchFilters = searchFilters,
            SelectedSexValues = selectedSexValues
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


    private static void AssertAbstractValues(NPDLearnerTextSearchController controller, LearnerTextSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
        Assert.Equal(controller.SearchController, model.LearnerTextSearchController);
        Assert.Equal(controller.SearchAction, model.LearnerTextSearchAction);
        Assert.Equal(controller.SearchLearnerNumberController, model.LearnerNumberController);
        Assert.Equal(controller.SearchLearnerNumberAction, model.LearnerNumberAction);
    }

    private NPDLearnerTextSearchController GetController(int maxMPLLimit = 4000)
    {
        ClaimsPrincipal user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumUPNsPerSearch = 4000,
            CommonTransferFileUPNLimit = 4000,
            DownloadOptionsCheckLimit = 500,
            NonUpnNPDMyPupilListLimit = maxMPLLimit,
            MaximumNonUPNResults = 100
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        DefaultHttpContext httpContextStub = new() { User = user, Session = _mockSession };
        TempDataDictionary mockTempData = new(httpContextStub, Substitute.For<ITempDataProvider>());

        List<AvailableDatasetResult> availableDatasetResults = new()
            {
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.KS1, HasData: true, CanDownload: true),
                new AvailableDatasetResult(Dataset: Core.Downloads.Application.Enums.Dataset.KS2, HasData: true, CanDownload: true)
            };
        GetAvailableDatasetsForPupilsResponse response = new(availableDatasetResults);

        Mock<IUseCase<GetAvailableDatasetsForPupilsRequest, GetAvailableDatasetsForPupilsResponse>> mockGetAvailableDatasetsForPupilsUseCase = new();
        mockGetAvailableDatasetsForPupilsUseCase.Setup(repo => repo.HandleRequestAsync(It.IsAny<GetAvailableDatasetsForPupilsRequest>()))
            .ReturnsAsync(response);

        return new NPDLearnerTextSearchController(
             _mockLogger,
             _mockAppOptions,
             _mockPaginatedService,
             _mockMplService,
             _mockSelectionManager,
             _mockCtfService,
             _mockSessionProvider.Object,
             _mockDownloadService,
             mockGetAvailableDatasetsForPupilsUseCase.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = httpContextStub
            },
            TempData = mockTempData
        };
    }

    /*https://bytelanguage.net/2020/07/31/writing-unit-test-for-model-validation/*/

    private static void MockModelState<TModel, TController>(TModel model, TController controller) where TController : ControllerBase
    {
        ValidationContext validationContext = new(model, null, null);
        List<ValidationResult> validationResults = [];
        Validator.TryValidateObject(model, validationContext, validationResults, true);
        foreach (ValidationResult validationResult in validationResults)
        {
            controller.ModelState.AddModelError(validationResult.MemberNames.First(), validationResult.ErrorMessage);
        }
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
