using DfE.GIAP.Common.Models.Common;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace DfE.GIAP.Web.Tests.TestDoubles;

public class GlossaryResultsFake
{
    public CommonResponseBody GetCommonResponseBody()
    {
        return new CommonResponseBody
        {
            Title = "Glossary Title Test",
            Body = "Glossary Body Test"
        };
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
