using DfE.GIAP.Core.Contents.Application.Models;
using DfE.GIAP.Core.Models.Glossary;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GlossaryViewModel
{
    public Content Response { get; set; }
    public List<MetaDataDownload> MetaDataDownloadList = new();
}
