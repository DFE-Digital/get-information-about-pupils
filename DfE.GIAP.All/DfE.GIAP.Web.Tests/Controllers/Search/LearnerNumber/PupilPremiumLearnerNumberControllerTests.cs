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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Search.LearnerNumber
{
    public class PupilPremiumLearnerNumberControllerTests : IClassFixture<PaginatedResultsFake>
    {
        private readonly ILogger<PupilPremiumLearnerNumberController> _mockLogger = Substitute.For<ILogger<PupilPremiumLearnerNumberController>>();
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

        public PupilPremiumLearnerNumberControllerTests(PaginatedResultsFake paginatedResultsFake)
        {
            _paginatedResultsFake = paginatedResultsFake;
        }

        #region Search

        [Fact]
        public async Task PupilPremium_returns_empty_page_when_first_navigated_to()
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
            var result = await sut.PupilPremium(null);

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
        public async Task PupilPremium_returns_search_page_when_returned_to()
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

            var result = await sut.PupilPremium(true);

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
        public async Task PupilPremium_returns_a_page_of_results_when_searched()
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

            var result = await sut.PupilPremium(inputModel, 0, "", "", true);

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
        public async Task PupilPremium_returns_another_page_of_results_when_navigated_to()
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

            var result = await sut.PupilPremium(inputModel, 1, "", "");

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
        public async Task PupilPremium_select_all_works()
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

            var result = await sut.PupilPremium(inputModel, 1, "", "");

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
        public async Task PupilPremium_deselect_all_works()
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

            var result = await sut.PupilPremium(inputModel, 1, "", "");

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
        public async Task PupilPremium_changes_selection_on_page_if_selections_are_different()
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

            var paginatedResponse = _paginatedResultsFake.GetValidLearners();

            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));
            _mockSession.SetString(
               _paginatedResultsFake.TotalSearchResultsSessionKey,
               _paginatedResultsFake.TotalSearchResultsSessionValue);
            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(new HashSet<string>() { "A203102209083" });

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            SetupPaginatedSearch(sut.IndexType, paginatedResponse);

            var result = await sut.PupilPremium(inputModel, 1, "", "", true);

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
        public async Task PupilPremium_shows_error_if_no_UPNs_inputted()
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

            var result = await sut.PupilPremium(inputModel, 0, "", "", true);

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
        public async Task PupilPremium_shows_invalid_UPNs_on_search_if_they_exist()
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

            var result = await sut.PupilPremium(inputModel, 0, "", "", true);

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
        public async Task PupilPremium_shows_not_found_UPNs_on_search_if_they_do_not_exist()
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

            var result = await sut.PupilPremium(inputModel, 0, "", "", false);

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
        public async Task PupilPremium_shows_duplicate_UPNs_on_search_if_they_exist()
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

            var result = await sut.PupilPremium(inputModel, 0, "", "", true);

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
        public async Task PupilPremium_populates_LearnerNumberIds_with_Id_when_UPN_0()
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

            var result = await sut.PupilPremium(true);

            // assert

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);

            Assert.Equal("123\nA203202811068", model.LearnerNumberIds);
            Assert.True(model.Learners.SequenceEqual(expectedLearners));
        }

        #endregion Search

        #region Sorting

        [Fact]
        public async Task PupilPremium_preserves_sort_settings_when_navigated_to()
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
            var result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

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
            var result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

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
            var result = await sut.PupilPremium(inputModel, 1, sortField, sortDirection);

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
        public async Task PupilPremium_preserves_sort_settings_in_session_if_returnToSearch_true()
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
            var result = await sut.PupilPremium(true);

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
        public async Task PupilPremium_updates_model_with_sorting_forename_asc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_forename_desc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_middlenames_asc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_middlenames_desc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_surname_asc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_surname_desc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_gender_asc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_gender_desc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_dob_asc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PupilPremium_updates_model_with_sorting_dob_desc_correctly()
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
            var result = await sut.PupilPremium(inputModel, 0, sortField, sortDirection, true);

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
        public async Task PPInvalidUpns_shows_invalid_upn_page_upns_only()
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

            var result = await sut.PPInvalidUPNs(inputModel);

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
        public async Task PPInvalidUpns_shows_invalid_upn_page_ids_and_upns()
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

            var result = await sut.PPInvalidUPNs(inputModel);

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
        public async Task PPInvalidUpns_shows_invalid_upn_page_ids_only()
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

            var result = await sut.PPInvalidUPNs(inputModel);

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
        public async Task PPInvalidUpnsConfirmation_goes_to_MPL_if_asked()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_MyPupilList
            };

            // act
            var sut = GetController();

            var result = await sut.PPInvalidUPNsConfirmation(inputModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.True(redirectResult.ActionName.Equals(Global.MyPupilListAction));
            Assert.True(redirectResult.ControllerName.Equals(Global.MyPupilListControllerName));
        }

        [Fact]
        public async Task PPInvalidUpnsConfirmation_goes_back_to_search_if_asked()
        {
            var upns = _paginatedResultsFake.GetUpnsWithInvalid();
            var inputModel = new InvalidLearnerNumberSearchViewModel()
            {
                SelectedInvalidUPNOption = Global.InvalidUPNConfirmation_ReturnToSearch
            };

            // act
            var sut = GetController();

            var result = await sut.PPInvalidUPNsConfirmation(inputModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.True(redirectResult.ActionName.Equals(sut.SearchAction));
        }

        [Fact]
        public async Task PPInvalidUpnsConfirmation_shows_error_if_no_selection_made()
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

            var result = await sut.PPInvalidUPNsConfirmation(inputModel);

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

            var formattedMPLItems = new List<MyPupilListItem>();
            foreach (var item in inputModel.SelectedPupil)
            {
                formattedMPLItems.Add(new MyPupilListItem(item, false));
            }

            // act
            var sut = GetController();

            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString(
               _paginatedResultsFake.TotalSearchResultsSessionKey,
               _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.PPAddToMyPupilList(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));

            await _mockMplService.Received().UpdateMyPupilList(
                Arg.Is<IEnumerable<MyPupilListItem>>(u => u.SequenceEqual(formattedMPLItems)),
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
               _paginatedResultsFake.TotalSearchResultsSessionValue);
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            var result = await sut.PPAddToMyPupilList(inputModel);

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

            var result = await sut.PPAddToMyPupilList(inputModel);

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

            var result = await sut.PPAddToMyPupilList(inputModel);

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

        #region Download

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
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());

            // act
            var sut = GetController();

            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

            // assert
            Assert.IsType<FileContentResult>(result);
        }

        [Fact]
        public async Task DownloadSelectedPupilPremiumData_returns_search_page_with_error_if_no_pupil_selected()
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

            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

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
        public async Task DownloadSelectedPupilPremiumData_redirects_to_error_page_if_download_null()
        {
            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(upns.FormatLearnerNumbers().ToHashSet<string>());

            // act
            var sut = GetController();

            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

            // assert
            Assert.IsType<RedirectToActionResult>(result);
            var redirectResult = result as RedirectToActionResult;

            Assert.True(redirectResult.ActionName.Equals(Routes.Application.Error));
            Assert.True(redirectResult.ControllerName.Equals(Routes.Application.Home));
        }

        [Fact]
        public async Task DownloadSelectedPupilPremiumData_with_empty_document_returns_to_search_page_with_errorDetails()
        {
            // arrange
            var newsPubCommonResponse = new CommonResponseBody()
            {
                Id = "0",
                Body = "test"
            };

            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(newsPubCommonResponse);
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
                    // Omit the byte array to force the error!
                });

            var upns = _paginatedResultsFake.GetUpns();
            var inputModel = new LearnerNumberSearchViewModel()
            {
                LearnerNumberIds = upns,
                SelectedPupil = _paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToList(),
                PageLearnerNumbers = String.Join(',', upns.FormatLearnerNumbers())
            };

            var sut = GetController();

            _mockSelectionManager.GetSelected(Arg.Any<string[]>()).Returns(_paginatedResultsFake.GetUpns().FormatLearnerNumbers().ToHashSet());
            _mockSession.SetString(sut.SearchSessionKey, _paginatedResultsFake.GetUpns());
            _mockSession.SetString("totalSearch", "20");
            SetupPaginatedSearchGetValidLearners(sut.IndexType);

            // act
            var result = await sut.ToDownloadSelectedPupilPremiumDataUPN(inputModel);

            // assert
            Assert.IsType<ViewResult>(result);
            var viewResult = result as ViewResult;

            Assert.IsType<LearnerNumberSearchViewModel>(viewResult.Model);
            var model = viewResult.Model as LearnerNumberSearchViewModel;

            AssertAbstractValues(sut, model);
            Assert.True(viewResult.ViewName.Equals(Global.SearchView));
            Assert.Contains(DownloadErrorMessages.NoDataForSelectedPupils, model.ErrorDetails);
        }

        #endregion Download

        private void AssertAbstractValues(PupilPremiumLearnerNumberController controller, LearnerNumberSearchViewModel model)
        {
            Assert.Equal(controller.PageHeading, model.PageHeading);
            Assert.Equal(controller.DownloadLinksPartial, model.DownloadLinksPartial);
            Assert.Equal(controller.InvalidUPNsConfirmationAction, model.InvalidUPNsConfirmationAction);
            Assert.Equal(controller.SearchAction, model.SearchAction);
            Assert.Equal(controller.FullTextLearnerSearchController, model.FullTextLearnerSearchController);
            Assert.Equal(controller.FullTextLearnerSearchAction, model.FullTextLearnerSearchAction);
        }

        private PupilPremiumLearnerNumberController GetController(int maxMPLLimit = 4000)
        {
            var user = new UserClaimsPrincipalFake().GetUserClaimsPrincipal();

            _mockAppSettings = new AzureAppSettings()
            {
                MaximumUPNsPerSearch = 4000,
                UpnPPMyPupilListLimit = maxMPLLimit
            };

            _mockAppOptions.Value.Returns(_mockAppSettings);
            _mockSession.SetString(BaseLearnerNumberController.MISSING_LEARNER_NUMBERS_KEY, JsonConvert.SerializeObject(new List<string>()));

            return new PupilPremiumLearnerNumberController(
                _mockLogger,
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
