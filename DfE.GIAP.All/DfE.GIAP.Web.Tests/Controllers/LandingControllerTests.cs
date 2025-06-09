using DfE.GIAP.Common.Constants;
using DfE.GIAP.Common.Constants.DsiConfiguration;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Helpers.Banner;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers
{
    public class LandingControllerTests : IClassFixture<UserClaimsPrincipalFake>
    {
        private readonly UserClaimsPrincipalFake _userClaimsPrincipalFake;
        private readonly IContentService _mockContentService = Substitute.For<IContentService>();
        private readonly ILatestNewsBanner _mockNewsBanner = Substitute.For<ILatestNewsBanner>();
        public LandingControllerTests(UserClaimsPrincipalFake userClaimsPrincipalFake)
        {
            _userClaimsPrincipalFake = userClaimsPrincipalFake;
        }

        [Fact]
        public async Task LandingController_Index_Should_Return_LandingPageData()
        {
            // Arrange
            var model = new LandingViewModel()
            {
                LandingResponse = new Core.Models.Common.CommonResponseBody()
                {
                    Title = "Title",
                    Body = "Body"
                },
                PlannedMaintenanceResponse = new Core.Models.Common.CommonResponseBody()
                {
                    Title = "Planned Maintenance",
                    Body = "Maintenance content"
                },
                PublicationScheduleResponse = new Core.Models.Common.CommonResponseBody()
                {
                    Title = "Publication Schedule",
                    Body = "Schedule content"
                },
                FAQResponse = new Core.Models.Common.CommonResponseBody()
                {
                    Title = "FAQ",
                    Body = "FAQ content"
                }
            };
            _mockContentService.GetContent(DocumentType.Landing).Returns(model.LandingResponse);
            _mockContentService.GetContent(DocumentType.PlannedMaintenance).Returns(model.PlannedMaintenanceResponse);
            _mockContentService.GetContent(DocumentType.PublicationSchedule).Returns(model.PublicationScheduleResponse);
            _mockContentService.GetContent(DocumentType.FAQ).Returns(model.FAQResponse);

            var controller = GetLandingController();

            // Act
            var result = await controller.Index().ConfigureAwait(false);

            // Assert
            var viewResult = Assert.IsAssignableFrom<ViewResult>(result);
            var viewModel = viewResult.Model as LandingViewModel;
            Assert.Equal(model.LandingResponse.Title, viewModel.LandingResponse.Title);
            Assert.Equal(model.LandingResponse.Body, viewModel.LandingResponse.Body);
            Assert.Equal(model.PlannedMaintenanceResponse.Title, viewModel.PlannedMaintenanceResponse.Title);
            Assert.Equal(model.PlannedMaintenanceResponse.Body, viewModel.PlannedMaintenanceResponse.Body);
            Assert.Equal(model.PublicationScheduleResponse.Title, viewModel.PublicationScheduleResponse.Title);
            Assert.Equal(model.PublicationScheduleResponse.Body, viewModel.PublicationScheduleResponse.Body);
            Assert.Equal(model.FAQResponse.Title, viewModel.FAQResponse.Title);
            Assert.Equal(model.FAQResponse.Body, viewModel.FAQResponse.Body);
        }

        [Fact]
        public void LandingController_IndexPost_Should_Redirect_To_NPD_Search_If_User_Is_Not_An_FE_User()
        {
            // Arrange
            var controller = GetLandingController();

            // Act
            var result = controller.IndexPost();

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal(Global.NPDAction, viewResult.ActionName);
            Assert.Equal(Global.NPDLearnerNumberSearchController, viewResult.ControllerName);
        }

        [Fact]
        public void LandingController_IndexPost_Should_Redirect_To_FE_Search_If_User_Is_An_FE_User()
        {
            // Arrange
            var controller = GetLandingController(true);

            // Act
            var result = controller.IndexPost();

            // Assert
            var viewResult = Assert.IsAssignableFrom<RedirectToActionResult>(result);
            Assert.Equal(Global.FELearnerNumberSearchAction, viewResult.ActionName);
            Assert.Equal(Global.FELearnerNumberSearchController, viewResult.ControllerName);
        }

        private LandingController GetLandingController(bool feUser = false)
        {
            ClaimsPrincipal user;

            if (feUser)
            {
                user = _userClaimsPrincipalFake.GetSpecificUserClaimsPrincipal(
                    OrganisationCategory.Establishment,
                    EstablishmentType.FurtherEducation,
                    Role.Approver,
                    18,
                    25);
            }
            else
            {
                user = _userClaimsPrincipalFake.GetUserClaimsPrincipal();
            }

            return new LandingController(_mockContentService, _mockNewsBanner)
            {
                ControllerContext = new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = user }
                }
            };
        }
    }
}
