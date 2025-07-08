using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace DfE.GIAP.Common.Helpers
{
    public static class ValidationHelper
    {
        public static bool UPNRegexValidator(string upn)
        {
            const string REGEX_UPN_TYPE_1 = "^[A-HJ-NP-RT-Z][0-9]{12}$";
            const string REGEX_UPN_TYPE_2 = "^[A-HJ-NP-RT-Z][0-9]{11}[A-HJ-NP-RT-Z]$";
            var matchUPNFirstType = Regex.Match(upn, REGEX_UPN_TYPE_1);
            var matchUPNSecondType = Regex.Match(upn, REGEX_UPN_TYPE_2);
            return matchUPNFirstType.Success || matchUPNSecondType.Success;
        }
        public static bool UPNLengthValidator(string upn)
        {
            return upn.Length == 13;
        }

        public static bool IsValidUpn(string upn)
        {
            string upnParam = SecurityHelper.SanitizeText(upn.ToString());

            if (!ValidationHelper.UPNLengthValidator(upn) || (!ValidationHelper.UPNRegexValidator(upn)))
            {
                return false;
            }
            return true;
        }

        public static bool ULNRegexValidator(string uln)
        {
            const string REGEX_ULN_TYPE_1 = "^[0-9]{10}$";
            var matchULNType = Regex.Match(uln, REGEX_ULN_TYPE_1);
            return matchULNType.Success;
        }

        public static bool ULNLengthValidator(string uln)
        {
            return uln.Length == 10;
        }

        public static bool IsValidUln(string uln)
        {
            var ulnSplit = uln.ToCharArray();
            int initialSumValue = 10;
            int totalValueCounter = 0;

            if (!ValidationHelper.ULNLengthValidator(uln) || (!ValidationHelper.ULNRegexValidator(uln)))
            {
                return false;
            }

            //Only check the first 9 characters
            for (int i = 0; i < 9; i++)
            {
                int.TryParse(ulnSplit[i].ToString(), out int value);

                if (value > 0)
                {
                    totalValueCounter += initialSumValue * value;
                }
                else
                {
                    totalValueCounter += 0;
                }
                initialSumValue--;
            }

            var remainder = totalValueCounter % 11;
            if (remainder == 0)
            {
                return false;
            }

            var checkSum = 10 - remainder;
            int.TryParse(ulnSplit[9].ToString(), out int parity);

            if (checkSum == parity)
            {
                return true;
            }
            return false;
        }

        public static List<string> GetDuplicates(List<string> upns)
        {
            var duplicates = upns.GroupBy(x => x)
                                     .Where(x => x.Count() > 1)
                                     .Select(y => y.Key)
                                     .ToList();
            return duplicates;
        }

        public static List<string> FormatUPNULNSearchInput(string upnUln)
        {
            return upnUln.Replace("\r", "").Trim().Split("\n").ToList();
        }
    }
}
