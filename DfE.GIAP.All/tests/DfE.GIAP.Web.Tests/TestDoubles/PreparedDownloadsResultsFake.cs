using DfE.GIAP.Domain.Models.PrePreparedDownloads;
using DfE.GIAP.Web.ViewModels.PrePreparedDownload;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;
using System.Text;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class PreparedDownloadsResultsFake
    {
        public PreparedDownloadsViewModel GetGetPrePreparedDownloadsDetails()
        {
            return new PreparedDownloadsViewModel() {PreparedDownloadFiles = GetPrePreparedDownloadsList() };
        }
        public List<PrePreparedDownloads> GetPrePreparedDownloadsList()
        {
            var list = new List<PrePreparedDownloads>();

            list.Add(new PrePreparedDownloads() { Name = "PrePrepare downlaod file Name", FileName = "PrePreparedDownloadTest File Name", Date = Convert.ToDateTime("08/06/2030"), Link = "PrePreparedDownload Test Link" });

            return list;
        }
        public FileStreamResult GetMetaDataFile()
        {
            var ms = new MemoryStream();

            return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
            {
                FileDownloadName = "PrePreparedDownloadTestFile.csv"
            };
        }
    }
}
