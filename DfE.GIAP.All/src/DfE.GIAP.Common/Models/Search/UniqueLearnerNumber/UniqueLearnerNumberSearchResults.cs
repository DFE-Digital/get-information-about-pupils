using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Search.UniqueLearnerNumber
{
    [ExcludeFromCodeCoverage]
    public class UniqueLearnerNumberSearchResults
    {
        public string ULN { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public char? Gender { get; set; }
        public DateTime? DOB { get; set; }
    }
}
