using DfE.GIAP.Domain.Models.Search;
using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Search.Learner
{
    [ExcludeFromCodeCoverage]
    public class Learner : IRbac
    {
        public string Id { get; set; }

        public string LearnerNumber { get; set; }

        public string LearnerNumberId { get; set; }

        public string LocalAuthority { get; set; }

        public string Surname { get; set; }

        public string Forename { get; set; }

        public string Middlenames { get; set; }

        public string Gender { get; set; }

        public string Sex { get; set; }

        public DateTime? DOB { get; set; }

        public bool Selected { get; set; }

        public string PupilPremium { get; set; }

        public bool Equals(Learner other)
        {
            if (other is null)
                return false;

            return LearnerNumber == other.LearnerNumber
                && LearnerNumberId == other.LearnerNumberId
                && Forename == other.Forename
                && Middlenames == other.Middlenames
                && Surname == other.Surname
                && DOB == other.DOB
                && LocalAuthority == other.LocalAuthority;
        }

        public override bool Equals(object obj) => Equals(obj as Learner);

        public override int GetHashCode()
        {
            return HashCode.Combine(LearnerNumber, LearnerNumberId, Forename, Middlenames, Surname, DOB, LocalAuthority);
        }
    }
}