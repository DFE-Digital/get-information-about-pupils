using DfE.GIAP.Common.Helpers.Rbac;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace DfE.GIAP.Common.Helpers
{
    public static class StringHelper
    {
        public static string[] FormatLearnerNumbers(this string upns)
        {
            if (string.IsNullOrEmpty(upns)) return null;
            return upns.Split(new[] { "\n", "\r", "\r\n" }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
        }

        public static string ToSearchText(this string upns)
        {
            if (string.IsNullOrEmpty(upns)) return null;
            var upnString = upns.Replace("\r", string.Empty)
                               .Trim()
                               .Replace("\n", ",");
            return upnString;
        }

        public static string ToDecryptedSearchText(this string upns)
        {
            if (string.IsNullOrEmpty(upns)) return null;
            var upnArray = upns.Replace("\r", string.Empty)
                               .Trim()
                               .Split("\n")
                               .ToArray();
            var unencryptedUpnArray = RbacHelper.DecryptUpnCollection(upnArray);
            var upnString = string.Join(',', unencryptedUpnArray);
            return upnString;
        }

        [ExcludeFromCodeCoverage]
        public static System.Uri ConvertToUri(this string path)
        {
            var uri = new System.Uri(path);
            return uri;
        }

        public static string StringValueOfEnum(Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        public static string GetMetaDataFileName(string input)
        {
            var index = input.LastIndexOf('/');
            if (index > 0)
                input = input.Substring(index + 1);

            return input;
        }

        public static string GetMetaDataName(string input)
        {
            var index = input.LastIndexOf('.');
            if (index > 0)
                input = input.Substring(0, index);

            return input;
        }

        public static string EliminateSanitizeDefaultText(this string input)
        {
            var returnText = input.Trim().Length > 0 && input.Equals("&nbsp;") ? string.Empty : input;

            return returnText;
        }
    }
}