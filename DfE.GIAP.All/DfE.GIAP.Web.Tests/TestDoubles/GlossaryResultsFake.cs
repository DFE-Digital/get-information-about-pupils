using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mime;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class GlossaryResultsFake
    {
        public GlossaryViewModel GetGlossaryDetails()
        {
            return new GlossaryViewModel() { Response = new CommonResponseBodyViewModel() { Title = "Glossary Title Test", Body = "Glossary Body Test" }, MetaDataDownloadList = GetMetaDataDetailsList() };
        }

        public CommonResponseBody GetCommonResponseBody()
        {
            return new CommonResponseBody
            {
                Title = "Glossary Title Test",
                Body = "Glossary Body Test"
            };
        }

        public List<MetaDataDownload> GetMetaDataDetailsList()
        {
            var list = new List<MetaDataDownload>();

            list.Add(new MetaDataDownload() { Name = "Test Name", FileName = "Test File Name", Date = DateTime.Now, Link = "Test Link" });

            return list;
        }

        public FileStreamResult GetMetaDataFile()
        {
            var ms = new MemoryStream();

            return new FileStreamResult(ms, MediaTypeNames.Text.Plain)
            {
                FileDownloadName = "Test.csv"
            };
        }
    }
}
