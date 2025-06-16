using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;

namespace DfE.GIAP.Web.Controllers;

public class ServiceTimeoutController
{
    private const int DefaultSessionTimeout = 20; // Aligned to default idle timeout.
    private readonly AzureAppSettings _appSettings;

    public ServiceTimeoutController(IOptions<AzureAppSettings> appSettings)
    {
        _appSettings = appSettings?.Value ??
            throw new ArgumentNullException(nameof(appSettings));
    }

    [HttpPost]
    [AllowWithoutConsent]
    public JsonResult KeepSessionAlive() => new JsonResult("SessionPersisted");

    [HttpGet]
    [AllowWithoutConsent]
    public JsonResult SessionTimeoutValue()
    {
        int configuredSessionTimeout = _appSettings?.SessionTimeout ?? DefaultSessionTimeout;
        if (configuredSessionTimeout is 0) // IOptions provides type defaults if missing, int default is 0
        {
            configuredSessionTimeout = DefaultSessionTimeout;
        }

        return new JsonResult(configuredSessionTimeout);
    }
}
