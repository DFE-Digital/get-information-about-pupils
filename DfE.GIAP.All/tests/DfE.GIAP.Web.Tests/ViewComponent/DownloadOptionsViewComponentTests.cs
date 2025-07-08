using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Search;
using DfE.GIAP.Web.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using static DfE.GIAP.Web.ViewComponents.DownloadOptionsViewComponent;

namespace DfE.GIAP.Web.Tests.ViewComponent
{
    public class DownloadOptionsViewComponentTests
    {
        [Fact]
        public void Invoke_creates_correct_view_model()
        {
            // arrange
            var downloadDataTypes = new List<SearchDownloadDataType>();
            var downloadFileType = DownloadFileType.CSV;
            var showTABDownloadType = true;

            // act
            var result = new DownloadOptionsViewComponent().Invoke(downloadDataTypes, downloadFileType, showTABDownloadType);

            // assert
            Assert.IsType<ViewViewComponentResult>(result);
            var viewComponentResult = result as ViewViewComponentResult;
            Assert.IsType<DownloadOptionsModel>(viewComponentResult.ViewData.Model);
            var model = viewComponentResult.ViewData.Model as DownloadOptionsModel;
            Assert.Equal(downloadDataTypes, model.DownloadTypes);
            Assert.Equal(downloadFileType, model.DownloadFileType);
            Assert.Equal(showTABDownloadType, model.ShowTABDownloadType);
        }
    }
}
