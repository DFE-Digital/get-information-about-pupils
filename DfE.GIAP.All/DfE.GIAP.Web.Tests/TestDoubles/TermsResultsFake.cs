using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class TermsResultsFake
    {
        public TermsOfUseViewModel GetTermsDetails()
        {
            return new TermsOfUseViewModel() { Response = new CommonResponseBody() { Title = "test Title", Body = "test Body" } };
        }
    }
}
