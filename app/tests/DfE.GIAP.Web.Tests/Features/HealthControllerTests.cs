using DfE.GIAP.Web.Features;
using DfE.GIAP.Web.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Tests.Features;

public class HealthControllerTests
{
    [Fact]
    public void Index_ReturnsOkResult()
    {
        HealthController controller = new HealthController();

        IActionResult result = controller.Index();

        OkObjectResult ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Ok", ok.Value);
    }

    [Fact]
    public void Index_HasAllowAnonymousAttribute()
    {
        Type type = typeof(HealthController);

        object[] attribute = type.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true);

        Assert.NotEmpty(attribute);
    }

    [Fact]
    public void Index_HasAllowWithoutConsentAttribute()
    {
        Type type = typeof(HealthController);

        object[] attribute = type.GetCustomAttributes(typeof(AllowWithoutConsentAttribute), inherit: true);

        Assert.NotEmpty(attribute);
    }
}
