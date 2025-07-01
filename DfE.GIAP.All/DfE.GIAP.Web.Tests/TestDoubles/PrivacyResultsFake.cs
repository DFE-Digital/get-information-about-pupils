using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class PrivacyResultsFake
    {
        public PrivacyViewModel GetPrivacyDetails()
        {
            return new PrivacyViewModel() { Response = new CommonResponseBody() { Title = "test Title", Body = "test Body" } };
        }
    }
}
