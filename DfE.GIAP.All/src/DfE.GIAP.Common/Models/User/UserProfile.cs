using DfE.GIAP.Domain.Models.MPL;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.User
{
    [ExcludeFromCodeCoverage]
    public class UserProfile
    {
        public string UserId { get; set; }
        public bool IsPupilListUpdated { get; set; } = false;
        public string[] PupilList { get; set; } = new string[] { };
        public IEnumerable<MyPupilListItem> MyPupilList { get; set; }
    }
}
