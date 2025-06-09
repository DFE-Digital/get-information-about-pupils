using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Service.Content;
using DfE.GIAP.Web.Middleware;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace DfE.GIAP.Web.Controllers;

public class TermsController : Controller
{
    private readonly IContentService _contentService;

    public TermsController(IContentService contentService)
    {
        _contentService = contentService ??
            throw new ArgumentNullException(nameof(contentService));
    }

    [AllowWithoutConsent]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        CommonResponseBody results = await _contentService.GetContent(DocumentType.TermOfUse).ConfigureAwait(false);

        var model = new TermsOfUseViewModel
        {
            Response = results
        };

        return View(model);
    }
}
