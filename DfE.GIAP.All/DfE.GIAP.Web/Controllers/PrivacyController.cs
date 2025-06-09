using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers;

public class PrivacyController : Controller
{
    private readonly IContentService _contentService;

    public PrivacyController(IContentService contentService)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
    }

    [AllowWithoutConsent]
    public async Task<IActionResult> Index()
    {
        CommonResponseBody results = await _contentService.GetContent(DocumentType.PrivacyNotice).ConfigureAwait(false);

        var model = new PrivacyViewModel
        {
            Response = results
        };

        return View(model);
    }
}
