using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.MPL
{
    [ExcludeFromCodeCoverage]
    public class MyPupilListItem
    {
        public string PupilId { get; set; }
        public bool IsMasked { get; set; }

        public MyPupilListItem(string pupilId, bool isMasked)
        {
            PupilId = pupilId;
            IsMasked = isMasked;
        }

        public override bool Equals(object obj)
        {
            if (obj is MyPupilListItem other)
            {
                return other.PupilId.Equals(this.PupilId);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.PupilId, this.IsMasked);
        }
    }
}
