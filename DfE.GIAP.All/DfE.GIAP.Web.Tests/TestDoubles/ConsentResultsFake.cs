using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Web.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class ConsentResultsFake
    {
        public ConsentViewModel GetConsent()
        {
            return new ConsentViewModel() { Response = new CommonResponseBody() { Title = "test Title", Body = "test Body" } };
        }
    }
}
