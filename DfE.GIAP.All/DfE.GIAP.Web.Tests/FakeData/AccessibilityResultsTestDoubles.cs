using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Tests.FakeData;

public static class AccessibilityResultsTestDoubles
{
    public static AccessibilityViewModel GetAccessibilityDetails()
    {
        Content content = GetFakeResponse();

        return new AccessibilityViewModel()
        {
            Response = content
        };
    }

    public static Content GetFakeResponse()
    {
        return new()
        {
            Title = "Test Title",
            Body = "Sample body"
        };
    }
}
