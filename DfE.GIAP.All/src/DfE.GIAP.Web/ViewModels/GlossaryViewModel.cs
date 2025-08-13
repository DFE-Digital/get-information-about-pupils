using DfE.GIAP.Core.Models.Glossary;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class GlossaryViewModel
{
    public List<MetaDataDownload> MetaDataDownloadList = new();
}
