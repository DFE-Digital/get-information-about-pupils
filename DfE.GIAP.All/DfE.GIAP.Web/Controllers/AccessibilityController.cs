using DfE.GIAP.Common.Enums;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Helper;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System;
using DfE.GIAP.Core.Models.Common;

namespace DfE.GIAP.Web.Controllers;

public class AccessibilityController : Controller
{
    private readonly IContentService _contentService;

    public AccessibilityController(IContentService contentService)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
    }


    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        CommonResponseBody results = await _contentService.GetContent(DocumentType.Accessibility).ConfigureAwait(false);

        var model = new AccessibilityViewModel
        {
            Response = results.ConvertToViewModel()
        };

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Report()
    {
        var results = await _contentService.GetContent(DocumentType.AccessibilityReport).ConfigureAwait(false);

        var model = new AccessibilityViewModel
        {
            Response = results.ConvertToViewModel()
        };

        return View(model);
    }
}
