using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class MyPupilListResults
    {
        public IList<PupilDetail> PupilList { get; set; }
        public string[] MyPupilListArray { get; set; }
    }
}
