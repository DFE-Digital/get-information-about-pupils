using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Editor
{
    [ExcludeFromCodeCoverage]
    public class Document
    {
        public int Id { get; set; }

        public string DocumentId { get; set; }

        public string DocumentName { get; set; }

        public int SortId { get; set; }

        public bool IsEnabled { get; set; }
    }
}
