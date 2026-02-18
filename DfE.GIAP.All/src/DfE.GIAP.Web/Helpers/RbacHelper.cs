using System.Text;
using DfE.GIAP.Web.Constants;
using DfE.GIAP.Web.Features.Search.LegacyModels;

namespace DfE.GIAP.Web.Helpers;

public static class RbacHelper
{
    public static List<T> CheckRbacRulesGeneric<T>(List<T> results, int statutoryLowAge, int statutoryHighAge, DateTime? from = null)
       where T : IRbac
    {
        // Rbac rules don't apply
        if (statutoryLowAge == 0 && statutoryHighAge == 0)
        {
            return results;
        }

        foreach (T item in results)
        {
            if (item.DOB != null)
            {
                int age = CalculateAge(item.DOB.Value, from);

                if ((age < statutoryLowAge || age > statutoryHighAge) && item.LearnerNumber != null)
                {
                    item.LearnerNumberId = EncodeUpn(item.LearnerNumberId);
                    item.LearnerNumber = Global.UpnMask;
                }
            }
            else
            {
                item.LearnerNumberId = EncodeUpn(item.LearnerNumberId);
                item.LearnerNumber = Global.UpnMask;
            }
        }

        return results;
    }

    public static string EncodeUpn(string learnerNumber)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(learnerNumber);

        string encodedString = Convert.ToBase64String(bytes);

        return encodedString + Global.EncodedSuffixMarker;
    }

    public static string DecodeUpn(string learnerNumber)
    {
        if (string.IsNullOrEmpty(learnerNumber))
        {
            return string.Empty;
        }

        learnerNumber = learnerNumber.Replace(Global.EncodedSuffixMarker, string.Empty);
        byte[] bytes = Convert.FromBase64String(learnerNumber);

        string decodedString = Encoding.UTF8.GetString(bytes);

        return decodedString;
    }

    public static IEnumerable<string> DecryptUpnCollection(IEnumerable<string> learnerNumbers)
    {
        IEnumerable<string> unencryptedLearnerNumbers = learnerNumbers.Where(l => !l.Contains(Global.EncodedSuffixMarker));
        IEnumerable<string> decryptedLearnerNumbers = from learner in learnerNumbers
                                                      where learner.Contains(Global.EncodedSuffixMarker)
                                                      select DecodeUpn(learner);

        IEnumerable<string> unionPageLearnerNumbers = unencryptedLearnerNumbers.Union(decryptedLearnerNumbers);

        return unionPageLearnerNumbers;
    }

    public static int CalculateAge(DateTime dob, DateTime? from = null)
    {
        DateTime dateCalc = from ?? DateTime.Today;

        int age = dateCalc.Year - dob.Year;
        if (dob.Date > dateCalc.AddYears(-age))
        {
            age--;
        }
        return age;
    }
}
