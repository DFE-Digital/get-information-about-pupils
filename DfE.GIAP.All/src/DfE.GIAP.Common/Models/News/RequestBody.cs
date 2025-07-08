using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.News
{
    [ExcludeFromCodeCoverage]
    public class RequestBody
    {
        public bool ARCHIVED { get; set; }
        public bool DRAFTS { get; set; }
    }
}
