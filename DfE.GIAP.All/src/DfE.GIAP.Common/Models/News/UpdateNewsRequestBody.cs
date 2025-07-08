using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.News
{
    [ExcludeFromCodeCoverage]
    public class UpdateNewsRequestBody
    {
        public string ID { get; set; }
        public int ACTION { get; set; }
    }
}
