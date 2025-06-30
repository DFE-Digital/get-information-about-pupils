using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Web.ViewModels;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public class ConsentResultsFake
{
    public ConsentViewModel GetConsent()
    {
        return new ConsentViewModel()
        {
            Response = new Content()
            {
                Title = "test Title",
                Body = "test Body"
            }
        };
    }
}
