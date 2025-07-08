using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Search;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DfE.GIAP.Web.ViewComponents
{
    public class DownloadOptionsViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(
            IEnumerable<SearchDownloadDataType> downloadDataTypes,
            DownloadFileType downloadFileType,
            bool showTABDownloadType)
        {
            return View(new DownloadOptionsModel()
            {
                DownloadTypes = downloadDataTypes,
                DownloadFileType = downloadFileType,
                ShowTABDownloadType = showTABDownloadType
            });
        }

        public class DownloadOptionsModel
        {
            public IEnumerable<SearchDownloadDataType> DownloadTypes { get; set; }
            public DownloadFileType DownloadFileType { get; set; }
            public bool ShowTABDownloadType { get; set; }
        }
    }
}
