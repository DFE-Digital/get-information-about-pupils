using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class CustomFilterText
    {
        public string Forename { get; set; }

        public string Middlename { get; set; }

        public string Surname { get; set; }

        public int DobDay { get; set; }

        public int DobMonth { get; set; }

        public int DobYear { get; set; }
    }
}
