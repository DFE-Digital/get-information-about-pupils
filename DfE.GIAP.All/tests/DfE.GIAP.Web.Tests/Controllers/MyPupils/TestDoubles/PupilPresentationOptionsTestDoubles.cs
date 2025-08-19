using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;
using DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Order;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupils;
public static class PupilPresentationOptionsTestDoubles
{
    public static PupilsPresentationOptions Create(string sortKey) => Create(sortKey, It.IsAny<SortDirection>());

    public static PupilsPresentationOptions Create(string sortKey, SortDirection sortDirection)
    {
        return new(
            Page: It.IsAny<int>(),
            SortBy: sortKey,
            SortDirection: sortDirection);
    }
}
