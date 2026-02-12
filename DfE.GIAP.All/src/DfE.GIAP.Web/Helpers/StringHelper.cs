using System.ComponentModel;
using System.Reflection;

namespace DfE.GIAP.Web.Helpers;

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
}
