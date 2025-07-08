using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.SecurityReports
{
    [ExcludeFromCodeCoverage]
    public class AcademyRequest
    {
        public List<string> DocTypes { get; set; }

        public string Id { get; set; }

        public bool IncludeEstablishments { get; set; }
    }
}
