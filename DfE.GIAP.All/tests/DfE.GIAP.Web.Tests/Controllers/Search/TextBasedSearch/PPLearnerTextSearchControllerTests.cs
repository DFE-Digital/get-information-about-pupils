using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Core.MyPupils.Application.UseCases.AddPupilsToMyPupils;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Download;
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

public class PPLearnerTextSearchControllerTests : IClassFixture<PaginatedResultsFake>, IClassFixture<SearchFiltersFakeData>
{
    private readonly ILogger<PPLearnerTextSearchController> _mockLogger = Substitute.For<ILogger<PPLearnerTextSearchController>>();
    private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
    private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
    private readonly ITextSearchSelectionManager _mockSelectionManager = Substitute.For<ITextSearchSelectionManager>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private AzureAppSettings _mockAppSettings = new();
    private readonly SessionFake _mockSession = new();
    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly SearchFiltersFakeData _searchFiltersFake;
    private readonly Mock<ISessionProvider> _mockSessionProvider = new();

    public PPLearnerTextSearchControllerTests(PaginatedResultsFake paginatedResultsFake, SearchFiltersFakeData searchFiltersFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
        _searchFiltersFake = searchFiltersFake;
    }

    [Fact]
    public async Task NonUpnPupilPremiumDatabase_returns_empty_page_when_first_navigated_to()
    {
        // Arrange
        PPLearnerTextSearchController sut = GetController();

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

        PPLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

    [Fact]
    public async Task NonUpnPupilPremiumDatabase_does_not_call_GetPage_if_model_state_not_valid()
    {
        // Arrange
        PPLearnerTextSearchController sut = GetController();

        // Act
        await sut.NonUpnPupilPremiumDatabase(new LearnerTextSearchViewModel(), string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);

        // Assert
        await _mockPaginatedService.DidNotReceive().GetPage(
            Arg.Any<string>(),
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
        PPLearnerTextSearchController sut = GetController();
        //override default user to make admin so Ids are not masked, not testing rbac rules for this test
        sut.ControllerContext.HttpContext.User = UserClaimsPrincipalFake.GetAdminUserClaimsPrincipal();

        _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

        PaginatedResponse response = new()
        {
            Learners =
            [
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
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(true);

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
    public async Task SexFilter_Returns_to_route_with_correct_gender_filter(string genderFilter)
    {
        // Arrange
        string searchText = "John Smith";

        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText,
                _searchFiltersFake.GetSearchFilters(),
                [genderFilter]);

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        // Act
        IActionResult result = await sut.SexFilter(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult);

        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.NonUpnSearchView, viewResult.ViewName);
        Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetValidLearners().Learners));
        Assert.Equal(model.SearchFilters.CurrentFiltersAppliedString, searchViewModel.SearchFilters.CurrentFiltersAppliedString);
        Assert.Equal(model.SelectedSexValues[0], genderFilter);
    }

    [Fact]
    public async Task SexFilter_returns_all_genders_when_no_gender_selected()
    {
        // Arrange
        string searchText = "Smith";
        LearnerTextSearchViewModel searchViewModel =
            SetupLearnerTextSearchViewModel(
                searchText,
                _searchFiltersFake.GetSearchFilters());

        searchViewModel.SearchFilters.CurrentFiltersAppliedString = @"[{ ""FilterName"":""Female"",""FilterType"":6}]";

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
    public async Task SexFilter_returns_all_genders_when_more_than_one_gender_deselected()
    {
        // Arrange
        string searchText = "Smith";

        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(
            searchText,
            _searchFiltersFake.GetSearchFilters(),
            null);

        searchViewModel.SearchFilters.CurrentFiltersAppliedString = @"[{""FilterName"":""Female"",""FilterType"":6}, {""FilterName"":""Male"",""FilterType"":6}]";

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
    public async Task PPAddToMyPupilList_redirects_to_InvalidUPNs_if_invalid_upn_selected()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpnsWithInvalid();
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        // Act
        IActionResult result = await sut.PPAddToMyPupilList(searchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
    }

    [Fact]
    public async Task PPAddToMyPupilList_sets_download_links_correctly_on_serach_reload()
    {
        // Arrange
        string searchText = "John Smith";
        string upn = _paginatedResultsFake.GetUpn();
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        // Act
        IActionResult result = await sut.ToDownloadSelectedPupilPremiumDataUPN(searchViewModel);

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DownloadPupilPremiumFile_downloads_file()
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

        _mockDownloadService.GetPupilPremiumCSVFile(
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           true,
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>(),
           Arg.Any<UserOrganisation>())
           .Returns(new ReturnFile()
           {
               FileName = "test",
               FileType = "csv",
               Bytes = []
           });

        PPLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadPupilPremiumFile(downloadViewModel);

        // assert
        Assert.IsType<FileContentResult>(result);
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

        _mockDownloadService.GetPupilPremiumCSVFile(
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           true,
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>(),
           Arg.Any<UserOrganisation>())
           .Returns(new ReturnFile()
           {
               FileName = null,
               FileType = null,
               Bytes = null
           });

        PPLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.DownloadPupilPremiumFile(downloadViewModel);

        // assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task DownloadFileConfirmationReturn_downloads_file_when_confirmation_given()
    {
        // arrange
        StarredPupilConfirmationViewModel StarredPupilConfirmationViewModel = new()
        {
            SelectedPupil = _paginatedResultsFake.GetUpn(),
            DownloadType = DownloadType.PupilPremium,
            ConfirmationGiven = true,
            ConfirmationError = false,
            ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction,
            CancelReturnAction = Global.PPDownloadCancellationReturnAction,
            LearnerNumbers = _paginatedResultsFake.GetUpn()
        };

        _mockDownloadService.GetPupilPremiumCSVFile(
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           Arg.Any<bool>(),
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>(),
           Arg.Any<UserOrganisation>())
           .Returns(new ReturnFile()
           {
               FileName = "test",
               FileType = "csv",
               Bytes = []
           });

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        // act
        IActionResult result = await sut.DownloadFileConfirmationReturn(StarredPupilConfirmationViewModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadFileConfirmationReturn_redirects_to_ConfirmationForStarredPupil_when_no_confirmation_given()
    {
        // arrange
        StarredPupilConfirmationViewModel StarredPupilConfirmationViewModel = new()
        {
            SelectedPupil = _paginatedResultsFake.GetUpn(),
            DownloadType = DownloadType.PupilPremium,
            ConfirmationGiven = false,
            ConfirmationError = true,
            ConfirmationReturnAction = Global.PPDownloadConfirmationReturnAction,
            CancelReturnAction = Global.PPDownloadCancellationReturnAction,
            LearnerNumbers = _paginatedResultsFake.GetUpn()
        };

        _mockDownloadService.GetPupilPremiumCSVFile(
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           Arg.Any<bool>(),
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>(),
           Arg.Any<UserOrganisation>())
           .Returns(new ReturnFile()
           {
               FileName = "test",
               FileType = "csv",
               Bytes = []
           });

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        _mockDownloadService.GetPupilPremiumCSVFile(
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

        PPLearnerTextSearchController sut = GetController();

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());
        _mockSelectionManager.GetSelectedFromSession().Returns(upn);

        PPLearnerTextSearchController sut = GetController();
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        sut.TempData = Substitute.For<ITempDataDictionary>();

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

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        _mockDownloadService.GetPupilPremiumCSVFile(
           Arg.Any<string[]>(),
           Arg.Any<string[]>(),
           Arg.Any<bool>(),
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>())
           .Returns(new ReturnFile()
           {
               FileName = "test",
               FileType = "csv",
               Bytes = fileBytes
           });

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());


        PPLearnerTextSearchController sut = GetController();
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

        PPLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        // act
        IActionResult result = await sut.DownloadCancellationReturn(new StarredPupilConfirmationViewModel());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        PPLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        // act
        IActionResult result = await sut.DownloadCancellationReturn(new StarredPupilConfirmationViewModel());

        // assert            
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.Equal(ApplicationLabels.DownloadSelectedPupilPremiumDataLink, model.DownloadSelectedLink);
        Assert.Equal(ApplicationLabels.AddSelectedToMyPupilListLink, model.AddSelectedToMyPupilListLink);

    }

    [Fact]
    public async Task PPNonUpnInvalidUPNs_returns_invalid_upn_page_upns_only()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn
        };

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        // act
        IActionResult result = await sut.PPNonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        InvalidLearnerNumberSearchViewModel vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.True(vm.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
    }

    [Fact]
    public async Task PPNonUpnInvalidUPNs_returns_invalid_upn_page_ids_and_upns()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn
        };

        IEnumerable<Learner> expectedLearners = _paginatedResultsFake.GetInvalidLearners().Learners.Concat(_paginatedResultsFake.GetValidLearners().Learners);

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetValidLearners());

        // act
        IActionResult result = await sut.PPNonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        InvalidLearnerNumberSearchViewModel vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.True(vm.Learners.SequenceEqual(expectedLearners));
    }

    [Fact]
    public async Task PPNonUpnInvalidUPNs_returns_invalid_upn_page_ids_only()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn
        };

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, new PaginatedResponse());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetInvalidLearners());

        // act
        IActionResult result = await sut.PPNonUpnInvalidUPNs(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);

        InvalidLearnerNumberSearchViewModel vm = Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.True(vm.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
    }

    [Fact]
    public async Task PPNonUpnInvalidUPNsConfirmation_redirects_to_my_pupil_list()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn,
            SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_MyPupilList
        };

        PPLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.PPNonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal(Global.MyPupilListControllerName, viewResult.ControllerName);
        Assert.Equal(Global.MyPupilListAction, viewResult.ActionName);
    }

    [Fact]
    public async Task PPNonUpnInvalidUPNsConfirmation_redirects_to_search()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn,
            SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_ReturnToSearch
        };

        PPLearnerTextSearchController sut = GetController();

        // act
        IActionResult result = await sut.PPNonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

        // Assert
        RedirectToActionResult viewResult = Assert.IsType<RedirectToActionResult>(result, exactMatch: false);
        Assert.Equal(Global.PPNonUpnAction, viewResult.ActionName);
    }

    [Fact]
    public async Task PPNonUpnInvalidUPNsConfirmation_returns_no_option_selected_validation_message()
    {
        // Arrange
        string upn = _paginatedResultsFake.GetInvalidUpn();
        InvalidLearnerNumberSearchViewModel invalidLearnerNumberSearchViewModel = new()
        {
            LearnerNumber = upn,
            SelectedInvalidUPNOption = string.Empty
        };

        PPLearnerTextSearchController sut = GetController();

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

        MockModelState(invalidLearnerNumberSearchViewModel, sut);

        // act
        IActionResult result = await sut.PPNonUpnInvalidUPNsConfirmation(invalidLearnerNumberSearchViewModel);

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
        Assert.Equal(Global.InvalidUPNsView, viewResult.ViewName);
        Assert.Single(sut.ViewData.ModelState["NoContinueSelection"]!.Errors);
    }

    [Theory]
    [InlineData("Forename", "asc")]
    [InlineData("Surname", "desc")]
    public async Task Sort_is_correctly_handled(string sortField, string sortDirection)
    {
        // arrange
        string searchText = "John Smith";
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        PPLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;

        string sortField = "Forename";
        string sortDirection = "asc";


        PPLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
        LearnerTextSearchViewModel searchViewModel = SetupLearnerTextSearchViewModel(searchText, _searchFiltersFake.GetSearchFilters());

        string sortField = "Forename";
        string sortDirection = "asc";

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

        PPLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        string? surnameFilter = null;
        string? middlenameFilter = null;
        string? forenameFilter = null;
        string? searchByRemove = null;
        string sortField = "Forename";
        string sortDirection = "asc";

        // act
        PPLearnerTextSearchController sut = GetController();
        _mockSession.SetString(sut.SearchSessionKey, searchText);
        _mockSession.SetString(sut.SearchFiltersSessionKey, JsonConvert.SerializeObject(searchViewModel.SearchFilters));

        _mockSession.SetString(sut.SortDirectionKey, sortDirection);
        _mockSession.SetString(sut.SortFieldKey, sortField);

        sut.ControllerContext.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        sut.ControllerContext.HttpContext.Request.Query.ContainsKey("reset").Returns(true);

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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

        PPLearnerTextSearchController sut = GetController();

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

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
                searchText, _searchFiltersFake.GetSearchFilters(), selectedSexValues: ["M"]);

        ITempDataDictionary mockTempDataDictionary = Substitute.For<ITempDataDictionary>();
        mockTempDataDictionary.Add("PersistedSelectedGenderFilters", searchByRemove);
        PPLearnerTextSearchController sut = GetController();
        sut.TempData = mockTempDataDictionary;

        _mockSession.SetString(sut.SortDirectionKey, "asc");
        _mockSession.SetString(sut.SortFieldKey, "Forename");

        SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Text, _paginatedResultsFake.GetValidLearners());

        // act
        IActionResult result = await sut.NonUpnPupilPremiumDatabase(searchViewModel, surnameFilter, middlenameFilter, forenameFilter, searchByRemove, "", "");

        // Assert
        ViewResult viewResult = Assert.IsType<ViewResult>(result);
        
        LearnerTextSearchViewModel model = Assert.IsType<LearnerTextSearchViewModel>(viewResult.Model);
        Assert.True(string.IsNullOrEmpty(model.SortField));
        Assert.True(string.IsNullOrEmpty(model.SortDirection));
    }

    private static LearnerTextSearchViewModel SetupLearnerTextSearchViewModel(
        string searchText,
        SearchFilters searchFilters,
        string[]? selectedSexValues = null)
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


    private static void AssertAbstractValues(PPLearnerTextSearchController controller, LearnerTextSearchViewModel model)
    {
        Assert.Equal(controller.PageHeading, model.PageHeading);
        Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
        Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
        Assert.Equal(controller.SearchController, model.LearnerTextSearchController);
        Assert.Equal(controller.SearchAction, model.LearnerTextSearchAction);
        Assert.Equal(controller.SearchLearnerNumberController, model.LearnerNumberController);
        Assert.Equal(controller.SearchLearnerNumberAction, model.LearnerNumberAction);
    }


    private PPLearnerTextSearchController GetController()
    {
        ClaimsPrincipal user = UserClaimsPrincipalFake.GetUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumUPNsPerSearch = 4000,
            DownloadOptionsCheckLimit = 500,
            MaximumNonUPNResults = 100
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        DefaultHttpContext httpContextStub = new() { User = user, Session = _mockSession };
        TempDataDictionary mockTempData = new(httpContextStub, Substitute.For<ITempDataProvider>());

        return new PPLearnerTextSearchController(
             _mockLogger,
             _mockAppOptions,
             _mockPaginatedService,
             _mockSelectionManager,
             _mockSessionProvider.Object,
             _mockDownloadService,
             new Mock<IUseCaseRequestOnly<AddPupilsToMyPupilsRequest>>().Object)
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
