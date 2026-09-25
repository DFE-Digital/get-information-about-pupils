using DfE.GIAP.Web.Enums;
using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using static DfE.GIAP.Web.ViewComponents.DownloadOptionsViewComponent;

namespace DfE.GIAP.Web.Tests.ViewComponent;

public sealed class DownloadOptionsViewComponentTests
{
    [Fact]
    public void Invoke_creates_correct_view_model()
    {
        // arrange
        List<SearchDownloadDataType> downloadDataTypes = new List<SearchDownloadDataType>();
        DownloadFileType downloadFileType = DownloadFileType.CSV;
        bool showTABDownloadType = true;

        // act
        IViewComponentResult result = new DownloadOptionsViewComponent().Invoke(downloadDataTypes, downloadFileType, showTABDownloadType);

        // assert
        ViewViewComponentResult viewComponentResult = Assert.IsType<ViewViewComponentResult>(result);
        Assert.NotNull(viewComponentResult.ViewData);

        DownloadOptionsModel model = Assert.IsType<DownloadOptionsModel>(viewComponentResult.ViewData.Model);
        Assert.Equal(downloadDataTypes, model.DownloadTypes);
        Assert.Equal(downloadFileType, model.DownloadFileType);
        Assert.Equal(showTABDownloadType, model.ShowTABDownloadType);
    }
}
