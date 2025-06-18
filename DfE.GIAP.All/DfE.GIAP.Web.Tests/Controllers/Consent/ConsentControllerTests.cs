using System;
using System.Threading.Tasks;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Common.Enums;
using DfE.GIAP.Common.Helpers.CookieManager;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Controllers;
using DfE.GIAP.Web.Helpers.Consent;
using DfE.GIAP.Web.Tests.FakeData;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.Consent;

public class ConsentControllerTests
{
    private readonly IContentService _contentService = Substitute.For<IContentService>();
    private readonly IExceptionHandlerPathFeature _exceptionPathFeature = Substitute.For<IExceptionHandlerPathFeature>();

    [Fact]
    public async Task Index_returns_the_consent_view()
    {

        // Arrange
        var consentResponse = new CommonResponseBody();
        _contentService.GetContent(DocumentType.Consent).Returns(consentResponse);
        var controller = GetConsentController();

        // Act
        var result = await controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;
        Assert.IsType<ConsentViewModel>(viewResult.Model);
        var viewModel = viewResult.Model as ConsentViewModel;
        Assert.Equal(consentResponse, viewModel.Response);
    }


    [Fact]
    public void Index_Post_redirects_to_start_when_consent_given()
    {
        // Arrange
        var controller = GetConsentController();
        var consentModel = new ConsentViewModel()
        {
            ConsentGiven = true
        };

        // Act
        var result = controller.Index(consentModel);


        // Assert
        Assert.IsType<RedirectResult>(result);
        var redirectResult = result as RedirectResult;
        Assert.Equal(Routes.Application.Home, redirectResult.Url);
        Assert.True(ConsentHelper.HasGivenConsent(controller.HttpContext));
    }

    [Fact]
    public void Index_Post_shows_error_when_consent_not_given()
    {
        // Arrange
        var controller = GetConsentController();
        var consentModel = new ConsentViewModel()
        {
            ConsentGiven = false
        };


        // Act
        var result = controller.Index(consentModel);

        // Assert
        Assert.IsType<ViewResult>(result);
        var viewResult = result as ViewResult;
        Assert.IsType<ConsentViewModel>(viewResult.Model);
        var viewModel = viewResult.Model as ConsentViewModel;
        Assert.True(viewModel.ConsentError);
    }

    private ConsentController GetConsentController()
    {
        var cookieManager = Substitute.For<ICookieManager>();
        var options = Substitute.For<IOptions<AzureAppSettings>>();

        var azureAppSettings = new AzureAppSettings()
        {
            IsSessionIdStoredInCookie = true
        };

        options.Value.Returns(azureAppSettings);

        _exceptionPathFeature.Error.Returns(new Exception("test"));
        _exceptionPathFeature.Path.Returns("/");

        var claimsPrincipal = new UserClaimsPrincipalFake().GetAdminUserClaimsPrincipal();
        var controllerContext = new ControllerContext()
        {
            HttpContext = new DefaultHttpContext()
            {
                User = claimsPrincipal,
                Session = new TestSession(),
            }
        };
        controllerContext.HttpContext.Features.Set(_exceptionPathFeature);

        return new ConsentController(options, _contentService, cookieManager)
        {
            ControllerContext = controllerContext
        };
    }
}
