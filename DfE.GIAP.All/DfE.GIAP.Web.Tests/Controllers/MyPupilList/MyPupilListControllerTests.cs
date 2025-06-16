using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Common.Helpers.Rbac;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Controllers.MyPupilList;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList;

[Trait("My Pupil List", "My Pupil List Controller Tests")]
public class MyPupilListControllerTests :
                                                IClassFixture<UserClaimsPrincipalFake>,
                                                IClassFixture<PaginatedResultsFake>
{
    private readonly ILogger<MyPupilListController> _mockLogger = Substitute.For<ILogger<MyPupilListController>>();
    private readonly IDownloadCommonTransferFileService _mockCtfService = Substitute.For<IDownloadCommonTransferFileService>();
    private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
    private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
    private readonly IMyPupilListService _mockMplService = Substitute.For<IMyPupilListService>();
    private readonly ISelectionManager _mockSelectionManager = Substitute.For<ISelectionManager>();
    private readonly ICommonService _mockCommonService = Substitute.For<ICommonService>();
    private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
    private AzureAppSettings _mockAppSettings = new AzureAppSettings();
    private readonly TestSession _mockSession = new TestSession();

    private readonly PaginatedResultsFake _paginatedResultsFake;
    private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;

    public MyPupilListControllerTests(PaginatedResultsFake paginatedResultsFake, UserClaimsPrincipalFake userClaimsPrincipalFake)
    {
        _paginatedResultsFake = paginatedResultsFake;
        _userClaimsPrincipalFake = userClaimsPrincipalFake;
    }

    #region Search

    [Fact]
    public async Task MyPupilList_returns_correct_view_when_first_navigated_to()
    {
        // arrange
        var upns = string.Join("\n", _paginatedResultsFake.GetValidLearners().Learners.Select(l => l.LearnerNumber)) + "\n" + string.Join("\n", _paginatedResultsFake.GetLearners(2).Learners.Select(l => l.LearnerNumber));
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetLearners(2));

        // act
        var result = await sut.Index();

        // assert

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(model.Upn, upns);
        Assert.Equal(0, model.PageNumber);
    }

    #region Sort Order

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_asc_correctly_applied_forename()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Ascending);

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderBy(x => x.Forename);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_desc_correctly_applied_forename()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderByDescending(x => x.Forename);
        var sut = GetController();

        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_asc_correctly_applied_middlenames()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Middlenames, AzureSearchSortDirections.Ascending);

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderBy(x => x.Middlenames);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_desc_correctly_applied_middlenames()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Middlenames, AzureSearchSortDirections.Descending);

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderByDescending(x => x.Middlenames);
        var sut = GetController();

        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_asc_correctly_applied_surname()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Surname, AzureSearchSortDirections.Ascending);

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderBy(x => x.Surname);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_desc_correctly_applied_surname()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Surname, AzureSearchSortDirections.Descending);

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderByDescending(x => x.Surname);
        var sut = GetController();

        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_asc_correctly_applied_gender()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Gender, AzureSearchSortDirections.Ascending);

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderBy(x => x.Gender);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_desc_correctly_applied_gender()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.Gender, AzureSearchSortDirections.Descending);

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderByDescending(x => x.Gender);
        var sut = GetController();

        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_asc_correctly_applied_dob()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.DOB, AzureSearchSortDirections.Ascending);

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderBy(x => x.DOB);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task MyPupilList_returns_pupils_with_sorting_desc_correctly_applied_dob()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var upnArray = upns.FormatLearnerNumbers();

        SetUpLearnerList(upnArray);
        var inputModel = GetInputModel(upns, AzureSearchFields.DOB, AzureSearchSortDirections.Descending);

        var expectedList = _paginatedResultsFake.GetValidLearners().Learners.OrderByDescending(x => x.DOB);
        var sut = GetController();

        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    #endregion Sort Order

    [Fact]
    public async Task MyPupilList_returns_another_page_of_results_when_navigated_to()
    {
        // arrange
        var expectedUPNs = string.Join("\n", _paginatedResultsFake.GetValidLearners().Learners.Select(l => l.LearnerNumber)) + "\n" + string.Join("\n", _paginatedResultsFake.GetLearners(30).Learners.Select(l => l.LearnerNumber));

        var upns = _paginatedResultsFake.GetUpns();

        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetLearners(30));

        // act

        var result = await sut.MyPupilList(inputModel, 1);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(expectedUPNs, model.Upn);
        Assert.Equal(1, model.PageNumber);
        model.Learners.AssertSelected(false);
    }

    [Fact]
    public async Task MyPupilList_select_all_works()
    {
        // arrange
        var upns = string.Join("\n", _paginatedResultsFake.GetValidLearners().Learners.Select(l => l.LearnerNumber));
        var upnArray = upns.FormatLearnerNumbers();

        var inputModel = GetInputModel(upns);
        inputModel.SelectAllNoJsChecked = "true";

        var paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(false);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnArray.ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, new PaginatedResponse());

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        model.Learners.AssertSelected(true);
        _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
        Assert.Equal(upns, model.Upn);
        Assert.Equal(0, model.PageNumber);
        Assert.True(model.ToggleSelectAll);
    }

    [Fact]
    public async Task MyPupilList_deselect_all_works()
    {
        // arrange
        var upns = string.Join("\n", _paginatedResultsFake.GetValidLearners().Learners.Select(l => l.LearnerNumber));
        var upnArray = upns.FormatLearnerNumbers();

        var inputModel = GetInputModel(upns);
        inputModel.SelectAllNoJsChecked = "false";

        var paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.ToggleSelectAll(true);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, paginatedResponse);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        model.Learners.AssertSelected(false);
        _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
        _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
        Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
        Assert.Equal(upns, model.Upn);
        Assert.Equal(0, model.PageNumber);
        Assert.False(model.ToggleSelectAll);
    }

    [Fact]
    public async Task MyPupilList_preserves_selection_if_returnToMPL_true()
    {
        // arrange
        var upns = string.Join("\n", _paginatedResultsFake.GetValidLearners().Learners.Select(l => l.LearnerNumber));

        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = new List<string>() { _paginatedResultsFake.GetUpn() };
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());
        var paginatedResponse = _paginatedResultsFake.GetValidLearners();

        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(Task.FromResult(_paginatedResultsFake.GetUpnsInMPL()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>() { _paginatedResultsFake.GetUpn() });
        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, paginatedResponse);

        // act
        var result = await sut.Index(true);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
        Assert.Single(model.Learners.Where(l => l.Selected == true));
        Assert.Equal(upns, model.Upn);
        Assert.Equal(0, model.PageNumber);
    }

    [Fact]
    public async Task MyPupilList_preserves_sort_settings_if_returnToMPL_true()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        SetUpLearnerList(upns.FormatLearnerNumbers());

        var sut = GetController();
        _mockSession.SetString(sut.SortFieldSessionKey, AzureSearchFields.Forename);
        _mockSession.SetString(sut.SortDirectionSessionKey, AzureSearchSortDirections.Ascending);

        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.Index(true);

        // assert

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Equal(AzureSearchFields.Forename, model.SortField);
        Assert.Equal(AzureSearchSortDirections.Ascending, model.SortDirection);
    }

    [Fact]
    public async Task MyPupilList_changes_selection_on_page_if_selections_are_different()
    {
        // arrange
        var upns = string.Join("\n", _paginatedResultsFake.GetValidLearners().Learners.Select(l => l.LearnerNumber));

        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = new List<string>() { _paginatedResultsFake.GetUpn() };

        var paginatedResponse = _paginatedResultsFake.GetValidLearners();

        _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>() { _paginatedResultsFake.GetUpn() });
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, paginatedResponse);

        // act
        var result = await sut.MyPupilList(inputModel, 0, true);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        _mockSelectionManager.Received().AddAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { _paginatedResultsFake.GetUpn() })));
        _mockSelectionManager.Received().RemoveAll(
            Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203202811068" })));
        Assert.Equal(SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()).TrimEnd('\n'), model.Upn);
        Assert.Equal(0, model.PageNumber);
    }

    [Fact]
    public async Task MyPupilList_shows_message_not_error_if_no_UPNs_in_pupil_list()
    {
        // arrange
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();

        // act
        var result = await sut.Index();

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Null(model.ErrorDetails);
        Assert.True(model.NoPupil);
    }

    [Fact]
    public async Task MyPupilList_shows_message_not_error_if_no_pupils_found()
    {
        // arrange
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, new PaginatedResponse());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, new PaginatedResponse());

        // act
        var result = await sut.Index();

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Null(model.ErrorDetails);
        Assert.True(model.NoPupil);
    }

    [Fact]
    public async Task MyPupilList_shows_invalid_UPNs_on_search_if_they_exist()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpnsWithInvalid();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetInvalidLearners());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Single(model.Invalid);
        Assert.Equal(2, model.Learners.Count());
        Assert.Equal(2, model.Total);
    }

    [Fact]
    public async Task Duplicate_UPN_starred_pupils_are_masked()
    {
        // arrange
        var inputModel = GetInputModel(_paginatedResultsFake.GetUpn());

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var paginatedResponse = _paginatedResultsFake.GetValidLearners();
        paginatedResponse.Learners[1].LearnerNumber = paginatedResponse.Learners[0].LearnerNumber;
        var sut = GetController();

        sut.ControllerContext.HttpContext.User = _userClaimsPrincipalFake.GetSpecificUserClaimsPrincipal("001", "00", "GIAPApprover", 2, 2);
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, paginatedResponse);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Equal(2, model.Learners.Count());
        Assert.All(model.Learners, learner => Assert.Equal(Global.UpnMask, learner.LearnerNumber));
    }

    [Fact]
    public async Task Starred_unique_record_with_NPD_and_PP_displayed_once()
    {
        // arrange
        var inputModel = GetInputModel(_paginatedResultsFake.GetUpn());

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var paginatedResponse = _paginatedResultsFake.GetLearners(1);
        var sut = GetController();

        sut.ControllerContext.HttpContext.User = _userClaimsPrincipalFake.GetSpecificUserClaimsPrincipal("001", "00", "GIAPApprover", 2, 2);
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, paginatedResponse);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Single(model.Learners);
        Assert.All(model.Learners, learner => Assert.Equal(Global.UpnMask, learner.LearnerNumber));
    }

    [Fact]
    public async Task Starred_unique_record_with_NPD_and_PP_correctly_sets_PupilPremium_label()
    {
        // arrange
        var inputModel = GetInputModel(_paginatedResultsFake.GetUpn());

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var sut = GetController();

        sut.ControllerContext.HttpContext.User = _userClaimsPrincipalFake.GetSpecificUserClaimsPrincipal("001", "00", "GIAPApprover", 2, 2);
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetLearners(1));
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetLearners(1));

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Single(model.Learners);
        Assert.All(model.Learners, learner => Assert.Equal("Yes", learner.PupilPremium));
    }

    [Fact]
    public async Task MyPupilList_populates_LearnerNumberId()
    {
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        var inputModel = GetInputModel(_paginatedResultsFake.GetUpn());

        var sut = GetController();
        //override default user to make admin so Ids are not masked, not testing rbac rules for this test
        sut.ControllerContext.HttpContext.User = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();

        var response = new PaginatedResponse()
        {
            Learners = new List<Learner>()
                    {
                        new Learner()
                        {
                            Id = "123",
                            LearnerNumber = "A203102209083",
                        },
                        new Learner()
                        {
                            Id = "456",
                            LearnerNumber = "A203202811068",
                        }
                    },
            Count = 2
        };
        var expectedLearners = new List<Learner>()
                    {
                        new Learner()
                        {
                            Id = "123",
                            LearnerNumber = "A203102209083",
                            LearnerNumberId = "A203102209083",
                        },
                        new Learner()
                        {
                            Id = "456",
                            LearnerNumber = "A203202811068",
                            LearnerNumberId = "A203202811068",
                        }
                    };
        SetupPaginatedSearch(AzureSearchIndexType.NPD, response);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, response);

        // act

        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(model.Learners.SequenceEqual(expectedLearners));
    }

    #endregion Search

    #region RemoveSelected

    [Fact]
    public async Task RemoveSelected_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.False(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task RemoveSelected_with_all_selected_on_page_zero_returns_mpl_page_zero()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 0;

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash, new HashSet<string>());
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
        Assert.Empty(model.Learners);
    }

    [Fact]
    public async Task RemoveSelected_with_all_selected_on_page_one_returns_mpl_page_zero()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(21);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 1;
        inputModel.Total = 21;
        const int expectedPageNumber = 0;

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash, new HashSet<string>());
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash.Take(2).ToHashSet(), new HashSet<string>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.SortField, model.SortField);
        Assert.Equal(inputModel.SortDirection, model.SortDirection);
        Assert.Equal(expectedPageNumber, model.PageNumber);
    }

    [Fact]
    public async Task RemoveSelected_with_not_all_selected_on_page_one_returns_mpl_page_one()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(22);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 1;
        inputModel.Total = 22;

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash, new HashSet<string>());
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash.Take(1).ToHashSet(), new HashSet<string>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.SortField, model.SortField);
        Assert.Equal(inputModel.SortDirection, model.SortDirection);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
    }

    [Fact]
    public async Task RemoveSelected_with_all_selected_returns_default_empty_mpl_page()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(21);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 1;
        inputModel.Total = 21;
        const int expectedPageNumber = 0;

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash, new HashSet<string>());
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash.Take(21).ToHashSet(), new HashSet<string>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(expectedPageNumber, model.PageNumber);
        Assert.Empty(model.Learners);
    }

    [Fact]
    public async Task RemoveSelected_with_empty_selected_pupils_list_returns_no_pupil_selected_current_mpl_page()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 0;

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>(), new HashSet<string>());
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        var expectedList = learnersResponse.Learners.OrderByDescending(x => x.Forename);

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
        Assert.True(expectedList.SequenceEqual(model.Learners));
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task RemoveSelected_with_null_selected_pupils_returns_no_pupil_selected_current_mpl_page()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Forename, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 0;

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(null, new HashSet<string>());
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        var expectedList = learnersResponse.Learners.OrderByDescending(x => x.Forename);

        // act
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
        Assert.True(expectedList.SequenceEqual(model.Learners));
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task RemoveSelected_returns_mpl_with_noPupilSelected_if_null_selected()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var inputModel = new MyPupilListViewModel
        {
            Upn = string.Empty,
            PageLearnerNumbers = "",
            SelectedPupil = new List<string>()
        };
        var selectedEmpty = new string[] { }.ToHashSet();
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(null, new HashSet<string>());

        // act
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task RemoveSelected_returns_mpl_with_noPupilSelected_if_selectedPupils_is_zero()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var inputModel = new MyPupilListViewModel
        {
            Upn = string.Empty,
            PageLearnerNumbers = "",
            SelectedPupil = new List<string>()
        };
        var selectedEmpty = new string[] { }.ToHashSet();
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(selectedEmpty, new HashSet<string>());

        // act
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);
        var result = await sut.RemoveSelected(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.True(model.NoPupilSelected);
    }

    #endregion RemoveSelected

    #region Download CTF

    [Fact]
    public async Task DownloadCommonTransferFileData_returns_data()
    {
        // arrange
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
                Bytes = new byte[0]
            });

        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

        var sut = GetController();

        // act
        var result = await sut.ToDownloadCommonTransferFileData(inputModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadCommonTransferFileData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadCommonTransferFileData(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.False(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadCommonTransferFileData_returns_search_page_with_error_if_over_ctf_limit()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController(4000, 1);
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadCommonTransferFileData(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(DownloadErrorMessages.UPNLimitExceeded, model.ErrorDetails);
    }

    [Fact]
    public async Task DownloadCommonTransferFileData_returns_to_search_page_if_download_null()
    {
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

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

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadCommonTransferFileData(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.True(model.ErrorDetails.Equals(DownloadErrorMessages.NoDataForSelectedPupils));
        Assert.Equal(Global.LearnerNumberLabel, model.LearnerNumberLabel);
    }

    [Fact]
    public async Task DownloadCommonTransferFileData_returns_mpl_page_with_sort_order_preserved_for_any_errors()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Middlenames, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 0;
        inputModel.SelectedPupil = upnsHash.ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash);
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

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

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        var expectedList = learnersResponse.Learners.OrderByDescending(x => x.Forename);

        // act
        var result = await sut.ToDownloadCommonTransferFileData(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.SortField, model.SortField);
        Assert.Equal(inputModel.SortDirection, model.SortDirection);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task ToDownloadCommonTransferFileData_returns_starred_pupil_confirmation_if_starred_pupil_selected()
    {
        var upns = _paginatedResultsFake.GetBase64EncodedUpn();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act

        var result = await sut.ToDownloadCommonTransferFileData(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var starredPupilViewModel = Assert.IsType<StarredPupilConfirmationViewModel>(viewResult.Model);
        Assert.Equal(Global.StarredPupilConfirmationView, viewResult.ViewName);
        Assert.Equal(DownloadType.CTF, starredPupilViewModel.DownloadType);
        Assert.Equal(upns, starredPupilViewModel.SelectedPupil);
    }

    [Fact]
    public async Task DownloadCommonTransferFileData_passes_all_selected_records_to_download()
    {
        // arrange
        var learnersResult = _paginatedResultsFake.GetLearners(40);
        var upns = string.Join("\n", learnersResult.Learners.Select(l => l.LearnerNumber));
        var inputModel = new MyPupilListViewModel()
        {
            Upn = upns,
            SelectedPupil = upns.FormatLearnerNumbers().Take(20).ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers().Take(20))
        };
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
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
                Bytes = new byte[0]
            });
        var sut = GetController();
        // act
        var result = await sut.ToDownloadCommonTransferFileData(inputModel);
        // assert
        await _mockCtfService.Received().GetCommonTransferFile(
           Arg.Is<string[]>(x => x.SequenceEqual(upns.FormatLearnerNumbers())),
           Arg.Any<string[]>(),
           Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>());
    }

    #endregion Download CTF

    #region Download NPD

    [Fact]
    public async Task ToDownloadSelectedNPDDataUpn_returns_to_search_page_with_error_if_no_pupil_selected()
    {
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        // act
        var result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.False(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task ToDownloadSelectedNPDDataUpn_returns_options_page_when_pupils_selected()
    {
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        var joinedSelectedPupils = string.Join(',', upns.FormatLearnerNumbers());

        var sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // act
        var result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.True(viewResult.ViewName.Equals(Global.MPLDownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == upns.FormatLearnerNumbers().Length);
        Assert.True(model.LearnerNumber.Equals(upns));
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_type_selected()
    {
        var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        var joinedSelectedPupils = string.Join(',', upns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

        // act
        var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.True(viewResult.ViewName.Equals(Global.MPLDownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == upns.Length);
        Assert.True(sut.TempData["ErrorDetails"].Equals(SearchErrorMessages.SelectOneOrMoreDataTypes));
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_download_type_selected()
    {
        var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        var joinedSelectedPupils = string.Join(',', upns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.None
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

        // act
        var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.True(viewResult.ViewName.Equals(Global.MPLDownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == upns.Length);
        Assert.True(sut.TempData["ErrorDetails"].Equals(SearchErrorMessages.SelectFileType));
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_download_data_exists()
    {
        var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        var joinedSelectedPupils = string.Join(',', upns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.CSV
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

        _mockDownloadService.GetCSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>())
            .Returns(new ReturnFile());

        // act
        var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);

        Assert.True(viewResult.ViewName.Equals(Global.MPLDownloadNPDOptionsView));
        Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
        Assert.True(model.SelectedPupilsCount == upns.Length);
        Assert.True(sut.TempData["ErrorDetails"].Equals(DownloadErrorMessages.NoDataForSelectedPupils));
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_redirects_to_MPL_if_SelectedPupils_empty()
    {
        // arrange
        var inputDownloadModel = new LearnerDownloadViewModel();

        var sut = GetController();
        sut.TempData = Substitute.For<ITempDataDictionary>();

        // act
        var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(redirectResult.ActionName.Equals(Global.MyPupilListAction));
        Assert.True(redirectResult.ControllerName.Equals(Global.MyPupilListControllerName));
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_redirects_to_error_page_if_download_null()
    {
        var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        var joinedSelectedPupils = string.Join(',', upns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.CSV
        };

        var sut = GetController();

        // act
        var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);

        Assert.True(redirectResult.ActionName.Equals(Routes.Application.Error));
        Assert.True(redirectResult.ControllerName.Equals(Routes.Application.Home));
    }

    [Fact]
    public async Task DownloadSelectedNationalPupilDatabaseData_returns_data()
    {
        var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
        var joinedSelectedPupils = string.Join(',', upns);

        var inputDownloadModel = new LearnerDownloadViewModel()
        {
            SelectedPupils = joinedSelectedPupils,
            SelectedPupilsCount = upns.Length,
            SelectedDownloadOptions = new string[0],
            DownloadFileType = DownloadFileType.CSV
        };

        ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
        TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
        ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

        var sut = GetController();
        sut.TempData = tempData;

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
                Bytes = new byte[0]
            });

        // act
        var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task ToDownloadSelectedNPDDataUPN_returns_starred_pupil_confirmation_if_starred_pupil_selected()
    {
        var upns = _paginatedResultsFake.GetBase64EncodedUpn();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var starredPupilViewModel = Assert.IsType<StarredPupilConfirmationViewModel>(viewResult.Model);
        Assert.Equal(Global.StarredPupilConfirmationView, viewResult.ViewName);
        Assert.Equal(DownloadType.NPD, starredPupilViewModel.DownloadType);
        Assert.Equal(upns, starredPupilViewModel.SelectedPupil);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremuimData_passes_all_selected_records_to_download()
    {
        // arrange
        var learnersResult = _paginatedResultsFake.GetLearners(40);
        var upns = string.Join("\n", learnersResult.Learners.Select(l => l.LearnerNumber));
        var inputModel = new MyPupilListViewModel()
        {
            Upn = upns,
            SelectedPupil = upns.FormatLearnerNumbers().Take(20).ToList(),
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers().Take(20))
        };
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
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
        var sut = GetController();
        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);
        // assert
        await _mockDownloadService.Received().GetPupilPremiumCSVFile(
           Arg.Is<string[]>(x => x.SequenceEqual(upns.FormatLearnerNumbers())),
           Arg.Any<string[]>(),
           Arg.Any<bool>(),
           Arg.Any<AzureFunctionHeaderDetails>(),
           Arg.Any<ReturnRoute>(),
           Arg.Any<UserOrganisation>());
    }

    #endregion Download NPD

    #region Download PP

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

        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        var sut = GetController();

        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        Assert.IsType<FileContentResult>(result);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_returns_search_page_with_error_if_file_empty()
    {
        // arrange
        _mockDownloadService.GetPupilPremiumCSVFile(
            Arg.Any<string[]>(),
            Arg.Any<string[]>(),
            Arg.Any<bool>(),
            Arg.Any<AzureFunctionHeaderDetails>(),
            Arg.Any<ReturnRoute>(),
            Arg.Any<UserOrganisation>()
            ).Returns(new ReturnFile());

        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(DownloadErrorMessages.NoDataForSelectedPupils, model.ErrorDetails);
        Assert.Equal(Global.LearnerNumberLabel, model.LearnerNumberLabel);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_returns_mpl_page_with_sort_order_preserved_for_any_errors()
    {
        // arrange
        var learnersResponse = _paginatedResultsFake.GetLearners(5);
        var upnsHash = learnersResponse.GetLearnerNumbers();
        var upns = string.Join("\n", upnsHash);
        var inputModel = GetInputModel(upns, AzureSearchFields.Middlenames, AzureSearchSortDirections.Descending);
        inputModel.PageNumber = 0;
        inputModel.SelectedPupil = upnsHash.ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnsHash);
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());
        _mockDownloadService.GetPupilPremiumCSVFile(
              Arg.Any<string[]>(),
              Arg.Any<string[]>(),
              Arg.Any<bool>(),
              Arg.Any<AzureFunctionHeaderDetails>(),
              Arg.Any<ReturnRoute>(),
              Arg.Any<UserOrganisation>()
              ).Returns(new ReturnFile());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, learnersResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, learnersResponse);

        var expectedList = learnersResponse.Learners.OrderByDescending(x => x.Forename);

        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.Equal(inputModel.SortField, model.SortField);
        Assert.Equal(inputModel.SortDirection, model.SortDirection);
        Assert.Equal(inputModel.PageNumber, model.PageNumber);
        Assert.True(expectedList.SequenceEqual(model.Learners));
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_returns_search_page_with_error_if_no_pupil_selected()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpnsWithInvalid();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        Assert.False(model.NoPupil);
        Assert.True(model.NoPupilSelected);
    }

    [Fact]
    public async Task DownloadSelectedPupilPremiumData_redirects_to_error_page_if_download_null()
    {
        var upns = _paginatedResultsFake.GetUpns();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = upns.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
        var sut = GetController();

        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.True(redirectResult.ActionName.Equals(Routes.Application.Error));
        Assert.True(redirectResult.ControllerName.Equals(Routes.Application.Home));
    }

    [Fact]
    public async Task ToDownloadSelectedPupilPremiumDataUPN_returns_starred_pupil_confirmation_if_starred_pupil_selected()
    {
        var upns = _paginatedResultsFake.GetBase64EncodedUpn();
        var inputModel = GetInputModel(upns);

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetValidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetValidLearners());

        // act
        var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var starredPupilViewModel = Assert.IsType<StarredPupilConfirmationViewModel>(viewResult.Model);
        Assert.Equal(Global.StarredPupilConfirmationView, viewResult.ViewName);
        Assert.Equal(DownloadType.PupilPremium, starredPupilViewModel.DownloadType);
        Assert.Equal(upns, starredPupilViewModel.SelectedPupil);
    }

    #endregion Download PP

    #region Selection

    [Fact]
    public async Task Invalid_UPNs_are_selectable()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpnsWithInvalid();
        var invalidUPN = _paginatedResultsFake.GetInvalidUpn();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = invalidUPN.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(invalidUPN.FormatLearnerNumbers().ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetInvalidLearners());

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Single(model.Invalid);
        Assert.Equal(2, model.Learners.Count());
        Assert.Equal(upns.FormatLearnerNumbers(), model.Upn.FormatLearnerNumbers());
        Assert.Equal(2, model.Total);
        _mockSelectionManager.Received().AddAll(Arg.Is<List<string>>(x => x.First().Equals(invalidUPN)));
        _mockSelectionManager.Received().AddAll(Arg.Is<List<string>>(x => x.Count.Equals(1)));
    }

    [Fact]
    public async Task Invalid_UPNs_are_selectable_with_starred_pupils()
    {
        // arrange
        var upns = _paginatedResultsFake.GetUpnsWithInvalid();
        var invalidUPN = _paginatedResultsFake.GetInvalidUpn();
        var inputModel = GetInputModel(upns);
        inputModel.SelectedPupil = invalidUPN.FormatLearnerNumbers().ToList();

        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(invalidUPN.FormatLearnerNumbers().ToHashSet());
        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

        var paginatedResponse = _paginatedResultsFake.GetInvalidLearners();
        var sut = GetController();
        var ageMinMax = RbacHelper.CalculateAge((DateTime)paginatedResponse.Learners.Last().DOB);
        sut.ControllerContext.HttpContext.User = _userClaimsPrincipalFake.GetSpecificUserClaimsPrincipal("001", "00", "GIAPApprover", ageMinMax, ageMinMax);
        SetupPaginatedSearch(AzureSearchIndexType.NPD, paginatedResponse);
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, paginatedResponse);

        // act
        var result = await sut.MyPupilList(inputModel, 0);

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);

        Assert.Single(model.Invalid);
        Assert.Equal(2, model.Learners.Count());
        //expect first upn to have been replaced with encoded upn due to age
        Assert.Equal(upns.FormatLearnerNumbers().Skip(1).Prepend(_paginatedResultsFake.GetBase64EncodedUpn()), model.Upn.FormatLearnerNumbers());
        Assert.Equal(2, model.Total);
        _mockSelectionManager.Received().AddAll(Arg.Is<List<string>>(x => x.First().Equals(invalidUPN)));
        _mockSelectionManager.Received().AddAll(Arg.Is<List<string>>(x => x.Count.Equals(1)));
    }

    [Fact]
    public async Task Invalid_UPNs_can_be_deselected()
    {
        // arrange
        var formattedMPLItems = new List<MyPupilListItem>();
        foreach (var item in _paginatedResultsFake.GetUpnsWithInvalid().FormatLearnerNumbers())
        {
            formattedMPLItems.Add(new MyPupilListItem(item, false));
        }

        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(formattedMPLItems);
        var sut = GetController();
        SetupPaginatedSearch(AzureSearchIndexType.NPD, _paginatedResultsFake.GetInvalidLearners());
        SetupPaginatedSearch(AzureSearchIndexType.PupilPremium, _paginatedResultsFake.GetInvalidLearners());

        // act
        var result = await sut.Index();

        // assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Routes.MyPupilList.MyPupilListView));
        var model = Assert.IsType<MyPupilListViewModel>(viewResult.Model);
        //invalid upn must be included in page learner numbers to be passed to selection manager on any action
        Assert.Contains(_paginatedResultsFake.GetInvalidUpn(), model.PageLearnerNumbers);
        Assert.Contains(_paginatedResultsFake.GetInvalidUpn(), model.Upn);
    }

    #endregion Selection

    #region Download file confirmation

    [Fact]
    public async Task DownloadFileConfirmationReturn_return_confirmation_page_when_not_confirmed()
    {
        // Arrange
        var inputModel = new StarredPupilConfirmationViewModel()
        {
            DownloadType = DownloadType.CTF,
            ConfirmationGiven = false,
            SelectedPupil = _paginatedResultsFake.GetUlns()
        };
        var sut = GetController();

        // Act
        var result = await sut.DownloadFileConfirmationReturn(inputModel);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.True(viewResult.ViewName.Equals(Global.StarredPupilConfirmationView));
    }

    #endregion Download file confirmation

    #region PopulateModelLearners

    [Fact]
    public void PopulateLearners_works_for_multiple_starred_pupils()
    {
        // arrange
        var learners = new List<Learner>()
            {
                new Learner()
                {
                    LearnerNumber = Global.UpnMask,
                    LearnerNumberId = RbacHelper.EncryptUpn("A203102209083"),
                },
                new Learner()
                {
                    LearnerNumber = Global.UpnMask,
                    LearnerNumberId = RbacHelper.EncryptUpn("A203202811068"),
                }
        };

        var inputModel = new MyPupilListViewModel();
        var sut = GetController();

        // act
        var result = sut.PopulateLearners(learners, inputModel, learners, 0);

        // assert
        Assert.Equal(learners, result.Learners);
        Assert.Empty(result.Invalid);
    }

    [Fact]
    public void PopulateLearners_works_with_invalid_pupils()
    {
        // arrange
        var learners = _paginatedResultsFake.GetInvalidLearners().Learners;

        var inputModel = new MyPupilListViewModel();
        var sut = GetController();

        // act
        var result = sut.PopulateLearners(learners, inputModel, learners, 0);

        // assert
        Assert.Equal(learners.Take(2), result.Learners);
        Assert.Single(result.Invalid);
    }

    [Fact]
    public void PopulateLearners_sets_PupilPremium_property_correctly()
    {
        // arrange
        var learners = _paginatedResultsFake.GetValidLearners().Learners;

        var inputModel = new MyPupilListViewModel();
        var sut = GetController();

        // act
        var result = sut.PopulateLearners(learners, inputModel, learners.Take(1).ToList(), 0);

        // assert
        Assert.Equal(learners, result.Learners);
        Assert.Empty(result.Invalid);
        Assert.Equal("Yes", result.Learners.First().PupilPremium);
        Assert.Equal("No", result.Learners.Last().PupilPremium);
    }

    #endregion PopulateModelLearners

    private MyPupilListController GetController(int maxMPLLimit = 4000, int CTFUPNLimit = 4000)
    {
        var user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();

        _mockAppSettings = new AzureAppSettings()
        {
            MaximumUPNsPerSearch = 4000,
            UpnNPDMyPupilListLimit = maxMPLLimit,
            CommonTransferFileUPNLimit = CTFUPNLimit
        };

        _mockAppOptions.Value.Returns(_mockAppSettings);

        var context = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext() { User = user, Session = _mockSession }
        };
        context.HttpContext.Request.Query = Substitute.For<IQueryCollection>();
        context.HttpContext.Request.Query.ContainsKey("pageNumber").Returns(true);

        return new MyPupilListController(
            _mockLogger,
            _mockPaginatedService,
            _mockMplService,
            _mockSelectionManager,
            _mockCtfService,
            _mockDownloadService,
            _mockCommonService,
            _mockAppOptions)
        {
            ControllerContext = context
        };
    }

    private void SetupPaginatedSearch(AzureSearchIndexType indexType, PaginatedResponse paginatedResponse)
    {
        _mockPaginatedService.GetPage(
        Arg.Any<string>(),
        Arg.Any<Dictionary<string, string[]>>(),
        Arg.Any<int>(),
        Arg.Any<int>(),
        Arg.Is(indexType),
        Arg.Is(AzureSearchQueryType.Numbers),
        Arg.Any<AzureFunctionHeaderDetails>(),
        Arg.Any<string>(),
        Arg.Any<string>())
        .Returns(paginatedResponse);
    }

    private void SetUpLearnerList(string[] upnArray)
    {
        var formattedMPLItems = new List<MyPupilListItem>();
        foreach (var item in upnArray)
        {
            formattedMPLItems.Add(new MyPupilListItem(item, false));
        }

        _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(formattedMPLItems);
        _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upnArray.ToHashSet());
    }

    private MyPupilListViewModel GetInputModel(string upns, string sortField = "", string sortDirection = "")
    {
        return new MyPupilListViewModel()
        {
            Upn = upns,
            PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers()),
            SortField = sortField,
            SortDirection = sortDirection
        };
    }
}
