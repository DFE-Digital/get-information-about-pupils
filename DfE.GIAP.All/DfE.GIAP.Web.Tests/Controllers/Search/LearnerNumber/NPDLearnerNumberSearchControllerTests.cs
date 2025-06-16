using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Common.Constants.Messages.Downloads;
using DfE.GIAP.Common.Constants.Messages.Search;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Domain.Models.Common;
using DfE.GIAP.Domain.Models.MPL;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Service.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Service.Download;
using DfE.GIAP.Service.Download.CTF;
using DfE.GIAP.Service.MPL;
using DfE.GIAP.Service.Search;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Controllers.LearnerNumber;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Helpers.SelectionManager;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.LearnerNumber
{
    public class NPDLearnerNumberSearchControllerTests : IClassFixture<PaginatedResultsFake>
    {
        private readonly ILogger<NPDLearnerNumberSearchController> _mockLogger = Substitute.For<ILogger<NPDLearnerNumberSearchController>>();
        private readonly IDownloadCommonTransferFileService _mockCtfService = Substitute.For<IDownloadCommonTransferFileService>();
        private readonly IDownloadService _mockDownloadService = Substitute.For<IDownloadService>();
        private readonly IPaginatedSearchService _mockPaginatedService = Substitute.For<IPaginatedSearchService>();
        private readonly IMyPupilListService _mockMplService = Substitute.For<IMyPupilListService>();
        private readonly ISelectionManager _mockSelectionManager = Substitute.For<ISelectionManager>();
        private readonly ICommonService _mockCommonService = Substitute.For<ICommonService>();
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();
        private readonly IOptions<AzureAppSettings> _mockAppOptions = Substitute.For<IOptions<AzureAppSettings>>();
        private AzureAppSettings _mockAppSettings = new AzureAppSettings();
        private readonly ILatestNewsBanner _mockNewsBanner = Substitute.For<ILatestNewsBanner>();

        private readonly TestSession _mockSession = new TestSession();

        private readonly PaginatedResultsFake _paginatedResultsFake;

        public NPDLearnerNumberSearchControllerTests(PaginatedResultsFake paginatedResultsFake)
        {
            _paginatedResultsFake = paginatedResultsFake;
        }

        #region Search

        [Fact]
        public async Task NationalPupilDatabase_returns_empty_page_when_first_navigated_to()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            // act
            var sut = GetController();
            var result = await sut.NationalPupilDatabase(null);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            _mockSelectionManager.Received().Clear();

            AssertAbstractValues(sut, model);

            Assert.Equal(model.DataReleaseTimeTable.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.DataReleaseTimeTable.NewsPublication.Body, newsPubCommonResponse.Body);
        }

        [Fact]
        public async Task NationalPupilDatabase_returns_search_page_when_returned_to()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NationalPupilDatabase(true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, _paginatedResultsFake.GetUpns());
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.DataReleaseTimeTable.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.DataReleaseTimeTable.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        }

        [Fact]
        public async Task NationalPupilDatabase_returns_a_page_of_results_when_searched()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));
        }

        [Fact]
        public async Task NationalPupilDatabase_returns_another_page_of_results_when_navigated_to()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                PageLearnerNumbers = String.Join(',', _paginatedResultsFake.GetUpns().FormatLearnerNumbers())
            };

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NationalPupilDatabase(inputModel, 1, "", "");

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            model.Learners.AssertSelected(true);
        }

        [Fact]
        public async Task NationalPupilDatabase_select_all_works()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectAllNoJsChecked = "true",
                SelectedPupil = new List<string>() { "A203102209083" },
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            var paginatedResponse = _paginatedResultsFake.GetValidLearners();
            paginatedResponse.ToggleSelectAll(false);

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, paginatedResponse);

            var result = await sut.NationalPupilDatabase(inputModel, 1, "", "");

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            model.Learners.AssertSelected(true);
            _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
            _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
            Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.True(model.ToggleSelectAll);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
        }

        [Fact]
        public async Task NationalPupilDatabase_deselect_all_works()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectAllNoJsChecked = "false",
                SelectedPupil = new List<string>() { "A203102209083" }
            };

            var paginatedResponse = _paginatedResultsFake.GetValidLearners();
            paginatedResponse.ToggleSelectAll(true);

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, paginatedResponse);

            var result = await sut.NationalPupilDatabase(inputModel, 1, "", "");

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            model.Learners.AssertSelected(false);
            _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
            _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
            Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.False(model.ToggleSelectAll);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
        }

        [Fact]
        public async Task NationalPupilDatabase_changes_selection_on_page_if_selections_are_different()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = new List<string>() { "A203102209083" },
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>() { "A203102209083" });

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NationalPupilDatabase(inputModel, 1, "", "", true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            _mockSelectionManager.Received().AddAll(
                Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203102209083" })));
            _mockSelectionManager.Received().RemoveAll(
                Arg.Is<IEnumerable<string>>(l => l.SequenceEqual(new List<string> { "A203202811068" })));
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
        }

        [Fact]
        public async Task NationalPupilDatabase_shows_error_if_no_UPNs_inputted()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel();

            // act
            var sut = GetController();
            sut.ModelState.AddModelError("test", "<span style='display:none'>1</span>");

            var result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.SearchBoxErrorMessage, SearchErrorMessages.EnterUPNs);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
        }

        [Fact]
        public async Task NationalPupilDatabase_shows_invalid_UPNs_on_search_if_they_exist()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = upns.FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);

            SetupPaginatedSearch(sut.IndexType, _paginatedResultsFake.GetInvalidLearners());

            var result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(model.Invalid.Count == 1);
            Assert.True(model.Learners.Count() == 3);
        }

        [Fact]
        public async Task NationalPupilDatabase_shows_not_found_UPNs_on_search_if_they_do_not_exist()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpnsWithNotFound();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = upns.FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NationalPupilDatabase(inputModel, 0, "", "", false);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(model.NotFound.Count == 1);
            Assert.True(model.Learners.Count() == 2);
        }

        [Fact]
        public async Task NationalPupilDatabase_shows_duplicate_UPNs_on_search_if_they_exist()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpnsWithDuplicates();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = upns.FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NationalPupilDatabase(inputModel, 0, "", "", true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(model.Duplicates.Count == 1);
            Assert.True(model.Learners.Count() == 2);
        }

        [Fact]
        public async Task NationalPupilDatabase_populates_LearnerNumberIds_with_Id_when_UPN_0()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            var response = new PaginatedResponse()
            {
                Learners = new List<Learner>()
                        {
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
                        },
                Count = 2
            };
            var expectedLearners = new List<Learner>()
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

            var result = await sut.NationalPupilDatabase(true);

            // assert

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

            Assert.Equal("123\nA203202811068", model.LearnerNumberIds);
            Assert.True(model.Learners.SequenceEqual(expectedLearners));
        }

        #endregion Search

        #region Sorting

        [Fact]
        public async Task NationalPupilDatabase_preserves_sort_settings_when_navigated_to()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                PageLearnerNumbers = String.Join(',', _paginatedResultsFake.GetUpns().FormatLearnerNumbers())
            };

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Forename";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 1, sortField, sortDirection);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            model.Learners.AssertSelected(true);

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task PupilUlnSearch_preserves_sort_settings_when_select_all_chosen()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectAllNoJsChecked = "true",
                SelectedPupil = new List<string>() { "A203102209083" },
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            var paginatedResponse = _paginatedResultsFake.GetValidLearners();
            paginatedResponse.ToggleSelectAll(false);

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, paginatedResponse);

            var sortField = "Forename";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 1, sortField, sortDirection);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            model.Learners.AssertSelected(true);
            _mockSelectionManager.Received().AddAll(Arg.Any<string[]>());
            _mockSelectionManager.DidNotReceive().RemoveAll(Arg.Any<string[]>());
            Assert.Equal(2, model.Learners.Where(l => l.Selected == true).Count());
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.True(model.ToggleSelectAll);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task PupilUlnSearch_preserves_sort_settings_if_deselect_all_chosen()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();

            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectAllNoJsChecked = "false",
                SelectedPupil = new List<string>() { "A203102209083" }
            };

            var paginatedResponse = _paginatedResultsFake.GetValidLearners();
            paginatedResponse.ToggleSelectAll(true);

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, paginatedResponse);

            var sortField = "Forename";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 1, sortField, sortDirection);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            model.Learners.AssertSelected(false);
            _mockSelectionManager.DidNotReceive().AddAll(Arg.Any<string[]>());
            _mockSelectionManager.Received().RemoveAll(Arg.Any<string[]>());
            Assert.Equal(2, model.Learners.Where(l => l.Selected == false).Count());
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(1, model.PageNumber);
            Assert.False(model.ToggleSelectAll);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_preserves_sort_settings_in_session_if_returnToSearch_true()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Forename";
            var sortDirection = "asc";
            _mockSession.SetString(sut.SearchSessionSortField, sortField);
            _mockSession.SetString(sut.SearchSessionSortDirection, sortDirection);
            var result = await sut.NationalPupilDatabase(true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, _paginatedResultsFake.GetUpns());
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.DataReleaseTimeTable.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.DataReleaseTimeTable.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.LearnerNumber.FormatLearnerNumbers().SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_forename_asc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Forename";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_forename_desc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Forename";
            var sortDirection = "desc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_middlenames_asc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "MiddleNames";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_middlenames_desc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "MiddleNames";
            var sortDirection = "desc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_surname_asc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Surname";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_surname_desc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Surname";
            var sortDirection = "desc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_gender_asc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Gender";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_gender_desc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Gender";
            var sortDirection = "desc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_dob_asc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Dob";
            var sortDirection = "asc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        [Fact]
        public async Task NationalPupilDatabase_updates_model_with_sorting_dob_desc_correctly()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var sortField = "Dob";
            var sortDirection = "desc";
            var result = await sut.NationalPupilDatabase(inputModel, 0, sortField, sortDirection, true);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.Equal(model.LearnerNumber, SecurityHelper.SanitizeText(_paginatedResultsFake.GetUpns()));
            Assert.Equal(0, model.PageNumber);
            Assert.Equal(model.NewsPublication.Id, newsPubCommonResponse.Id);
            Assert.Equal(model.NewsPublication.Body, newsPubCommonResponse.Body);
            Assert.True(model.SelectedPupil.SequenceEqual(_paginatedResultsFake.GetUpns().FormatLearnerNumbers()));

            Assert.Equal(model.SortField, sortField);
            Assert.Equal(model.SortDirection, sortDirection);
        }

        #endregion Sorting

        #region Invalid UPNs

        [Fact]
        public async Task NPDInvalidUpns_shows_invalid_upn_page_upns_only()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upns
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, new PaginatedResponse());

            var result = await sut.NPDInvalidUPNs(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.InvalidUPNsView));

            Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as InvalidLearnerNumberSearchViewModel;
            Assert.True(model.Learners.Count() == 3);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
        }

        [Fact]
        public async Task NPDInvalidUpns_shows_invalid_upn_page_ids_and_upns()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upns
            };
            var expectedLearners = _paginatedResultsFake.GetInvalidLearners().Learners.Concat(_paginatedResultsFake.GetValidLearners().Learners);

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, _paginatedResultsFake.GetInvalidLearners());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetValidLearners());

            var result = await sut.NPDInvalidUPNs(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.InvalidUPNsView));

            Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as InvalidLearnerNumberSearchViewModel;
            Assert.True(model.Learners.Count() == 5);
            Assert.True(model.Learners.SequenceEqual(expectedLearners));
        }

        [Fact]
        public async Task NPDInvalidUpns_shows_invalid_upn_page_ids_only()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upns
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Numbers, new PaginatedResponse());
            SetupPaginatedSearch(sut.IndexType, AzureSearchQueryType.Id, _paginatedResultsFake.GetInvalidLearners());

            var result = await sut.NPDInvalidUPNs(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.InvalidUPNsView));

            Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as InvalidLearnerNumberSearchViewModel;
            Assert.True(model.Learners.Count() == 3);
            Assert.True(model.Learners.SequenceEqual(_paginatedResultsFake.GetInvalidLearners().Learners));
        }

        [Fact]
        public async Task NPDInvalidUpnsConfirmation_goes_to_MPL_if_asked()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_MyPupilList
            };

            // act
            var sut = GetController();

            var result = await sut.NPDInvalidUPNsConfirmation(inputModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.True(redirectResult.ActionName.Equals(Global.MyPupilListAction));
            Assert.True(redirectResult.ControllerName.Equals(Global.MyPupilListControllerName));
        }

        [Fact]
        public async Task NPDInvalidUpnsConfirmation_goes_back_to_search_if_asked()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_ReturnToSearch
            };

            // act
            var sut = GetController();

            var result = await sut.NPDInvalidUPNsConfirmation(inputModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.True(redirectResult.ActionName.Equals(sut.SearchAction));
        }

        [Fact]
        public async Task NPDInvalidUpnsConfirmation_shows_error_if_no_selection_made()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                LearnerNumber = upns
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

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

            var result = await sut.NPDInvalidUPNsConfirmation(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.InvalidUPNsView));
            Assert.False(sut.ModelState.IsValid);
        }

        #endregion Invalid UPNs

        #region MPL

        [Fact]
        public async Task AddToMyPupilList_adds_to_mpl()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);

            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NPDAddToMyPupilList(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            await _mockMplService.Received().UpdateMyPupilList(
                Arg.Is<IEnumerable<MyPupilListItem>>(u => u.SequenceEqual(_paginatedResultsFake.GetUpnsInMPL())),
                Arg.Any<string>(),
                Arg.Any<AzureFunctionHeaderDetails>()
                );

            Assert.True(model.ItemAddedToMyPupilList);
        }

        [Fact]
        public async Task AddToMyPupilList_returns_search_page_with_error_if_no_pupil_selected()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());

            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue); SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NPDAddToMyPupilList(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.True(model.NoPupil);
            Assert.True(model.NoPupilSelected);
        }

        [Fact]
        public async Task AddToMyPupilList_redirects_to_InvalidUPNs_if_they_exist()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet());
            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NPDAddToMyPupilList(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<InvalidLearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as InvalidLearnerNumberSearchViewModel;

            Assert.True(viewResult.ViewName.Equals(Global.InvalidUPNsView));

            await _mockMplService.Received().UpdateMyPupilList(
                 Arg.Is<IEnumerable<MyPupilListItem>>(u => u.SequenceEqual(_paginatedResultsFake.GetUpnsInMPL())),
                 Arg.Any<string>(),
                 Arg.Any<AzureFunctionHeaderDetails>()
                 );
        }

        [Fact]
        public async Task AddToMyPupilList_returns_an_error_if_over_limit()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet<string>());
            _mockMplService.GetMyPupilListLearnerNumbers(Arg.Any<string>()).Returns(new List<MyPupilListItem>());

            // act
            var sut = GetController(1);

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.NPDAddToMyPupilList(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.True(model.ErrorDetails.Equals(CommonErrorMessages.MyPupilListLimitExceeded));
        }

        #endregion MPL

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
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            var result = await sut.DownloadCommonTransferFileData(inputModel);

            // assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task DownloadCommonTransferFileData_returns_search_page_with_error_if_no_pupil_selected()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.DownloadCommonTransferFileData(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.True(model.NoPupil);
            Assert.True(model.NoPupilSelected);
        }

        [Fact]
        public async Task DownloadCommonTransferFileData_returns_to_search_page_if_download_null()
        {
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet<string>());

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

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.DownloadCommonTransferFileData(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.True(model.ErrorDetails.Equals(DownloadErrorMessages.NoDataForSelectedPupils));
        }

        [Fact]
        public async Task DownloadCommonTransferFileData_exceeding_commonTransferFileUPNLimit_returns_to_search_page_with_errorDetails()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
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
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = string.Join(',', upns.FormatLearnerNumbers())
            };

            var sut = GetController(commonTransferFileUPNLimit: 1);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());
            _mockSession.SetString("totalSearch", "20");

            // act
            var result = await sut.DownloadCommonTransferFileData(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));
            Assert.Contains(DownloadErrorMessages.UPNLimitExceeded, model.ErrorDetails);
        }

        #endregion Download CTF

        #region Download NPD

        [Fact]
        public async Task ToDownloadSelectedNPDDataUpn_returns_to_search_page_with_error_if_no_pupil_selected()
        {
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);

            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>());

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
                _paginatedResultsFake.TotalSearchResultsSessionKey,
                _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.True(model.NoPupil);
            Assert.True(model.NoPupilSelected);
        }

        [Fact]
        public async Task ToDownloadSelectedNPDDataUpn_returns_options_page_when_pupils_selected()
        {
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                LearnerNumber = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet<string>());

            var joinedSelectedPupils = String.Join(',', upns.FormatLearnerNumbers());

            _mockDownloadService.CheckForNoDataAvailable(
                Arg.Any<string[]>(), Arg.Any<string[]>(), Arg.Any<string[]>(), Arg.Any<AzureFunctionHeaderDetails>())
                .Returns(new List<CheckDownloadDataType>() { CheckDownloadDataType.EYFSP });

            // act
            var sut = GetController();
            sut.TempData = Substitute.For<ITempDataDictionary>();
            var result = await sut.ToDownloadSelectedNPDDataUPN(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerDownloadViewModel;

            Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
            Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
            Assert.True(model.SelectedPupilsCount == upns.FormatLearnerNumbers().Length);
            Assert.True(model.LearnerNumber.Equals(upns));
            Assert.True(
                model.SearchDownloadDatatypes.Single(
                    d => d.Value.Equals(CheckDownloadDataType.EYFSP.ToString())
                    ).Disabled
                );
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_redirects_to_npd_search_if_SelectedPupils_empty()
        {
            // arrange
            var inputDownloadModel = new LearnerDownloadViewModel();

            var sut = GetController();
            sut.TempData = Substitute.For<ITempDataDictionary>();

            // act
            var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;
            Assert.True(redirectResult.ActionName.Equals(Global.NPDLearnerNumberSearchAction));
            Assert.True(redirectResult.ControllerName.Equals(Global.NPDLearnerNumberSearchController));
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_type_selected()
        {
            var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
            var joinedSelectedPupils = String.Join(',', upns);

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
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerDownloadViewModel;

            Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
            Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
            Assert.True(model.SelectedPupilsCount == upns.Length);
            Assert.True(sut.TempData["ErrorDetails"].Equals(SearchErrorMessages.SelectOneOrMoreDataTypes));
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_download_type_selected()
        {
            var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
            var joinedSelectedPupils = String.Join(',', upns);

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
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerDownloadViewModel;

            Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
            Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
            Assert.True(model.SelectedPupilsCount == upns.Length);
            Assert.True(sut.TempData["ErrorDetails"].Equals(SearchErrorMessages.SelectFileType));
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_returns_to_options_page_if_no_download_data_exists()
        {
            var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
            var joinedSelectedPupils = String.Join(',', upns);

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
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerDownloadViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerDownloadViewModel;

            Assert.True(viewResult.ViewName.Equals(Global.DownloadNPDOptionsView));
            Assert.True(model.SelectedPupils.Equals(joinedSelectedPupils));
            Assert.True(model.SelectedPupilsCount == upns.Length);
            Assert.True(sut.TempData["ErrorDetails"].Equals(DownloadErrorMessages.NoDataForSelectedPupils));
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_redirects_to_error_page_if_download_null()
        {
            var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
            var joinedSelectedPupils = String.Join(',', upns);

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
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.True(redirectResult.ActionName.Equals(Routes.Application.Error));
            Assert.True(redirectResult.ControllerName.Equals(Routes.Application.Home));
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_with_csv_type_returns_csv_data()
        {
            var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
            var joinedSelectedPupils = String.Join(',', upns);

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
                    FileName = "test_csv",
                    FileType = FileType.ZipFile,
                    Bytes = new byte[0]
                });

            // act
            var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

            // assert
            Assert.IsType<FileContentResult>(result);

            // Make sure the right call to download csv file has been made.
            await _mockDownloadService.Received().GetCSVFile(
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<bool>(),
                Arg.Any<AzureFunctionHeaderDetails>(),
                Arg.Any<ReturnRoute>()).ConfigureAwait(false);
        }

        [Fact]
        public async Task DownloadSelectedNationalPupilDatabaseData_with_tab_type_returns_tab_data()
        {
            var upns = _paginatedResultsFake.GetUpns().FormatLearnerNumbers();
            var joinedSelectedPupils = String.Join(',', upns);

            var inputDownloadModel = new LearnerDownloadViewModel()
            {
                SelectedPupils = joinedSelectedPupils,
                SelectedPupilsCount = upns.Length,
                SelectedDownloadOptions = new string[0],
                DownloadFileType = DownloadFileType.TAB
            };

            ITempDataProvider tempDataProvider = Substitute.For<ITempDataProvider>();
            TempDataDictionaryFactory tempDataDictionaryFactory = new TempDataDictionaryFactory(tempDataProvider);
            ITempDataDictionary tempData = tempDataDictionaryFactory.GetTempData(new DefaultHttpContext());

            var sut = GetController();
            sut.TempData = tempData;

            _mockDownloadService.GetTABFile(
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<bool>(),
                Arg.Any<AzureFunctionHeaderDetails>(),
                Arg.Any<ReturnRoute>())
                .Returns(new ReturnFile()
                {
                    FileName = "test_tab",
                    FileType = FileType.ZipFile,
                    Bytes = new byte[0]
                });

            // act
            var result = await sut.DownloadSelectedNationalPupilDatabaseData(inputDownloadModel);

            // assert
            Assert.IsType<FileContentResult>(result);

            // Make sure the right call to download tab file has been made.
            await _mockDownloadService.Received().GetTABFile(
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<string[]>(),
                Arg.Any<bool>(),
                Arg.Any<AzureFunctionHeaderDetails>(),
                Arg.Any<ReturnRoute>()).ConfigureAwait(false);
        }

        #endregion Download NPD

        private void AssertAbstractValues(NPDLearnerNumberSearchController controller, LearnerNumberSearchViewModel model)
        {
            Assert.Equal(controller.PageHeading, model.PageHeading);
            Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
            Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
            Assert.Equal(controller.SearchAction, model.SearchAction);
            Assert.Equal(controller.FullTextLearnerSearchController, model.FullTextLearnerSearchController);
            Assert.Equal(controller.FullTextLearnerSearchAction, model.FullTextLearnerSearchAction);
        }

        private NPDLearnerNumberSearchController GetController(int maxMPLLimit = 4000, int commonTransferFileUPNLimit = 4000)
        {
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

            _mockAppSettings = new AzureAppSettings()
            {
                MaximumUPNsPerSearch = 4000,
                UpnNPDMyPupilListLimit = maxMPLLimit,
                CommonTransferFileUPNLimit = commonTransferFileUPNLimit,
                DownloadOptionsCheckLimit = 500
            };

            _mockAppOptions.Value.Returns(_mockAppSettings);
            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));

            return new NPDLearnerNumberSearchController(
                _mockLogger,
                _mockCtfService,
                _mockDownloadService,
                _mockPaginatedService,
                _mockMplService,
                _mockSelectionManager,
                _mockContentService,
                _mockAppOptions)
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
}
