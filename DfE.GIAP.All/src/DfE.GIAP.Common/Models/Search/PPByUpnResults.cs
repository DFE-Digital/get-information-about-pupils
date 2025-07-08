using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class UpnResults
    {
        public string UPN { get; set; }
        public string Forename { get; set; }
        public string Surname { get; set; }
        public char? Gender { get; set; }
        public DateTime? DOB { get; set; }
    }
}
