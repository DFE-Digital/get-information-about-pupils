using System.Globalization;

namespace DfE.GIAP.Core.Common.Application.Helpers;

public static class DateFormatting
{
    public const string StandardDateFormat = "dd/MM/yyyy";
    public static string? ToStandardDate(DateTime? date) => date?.ToString(StandardDateFormat, CultureInfo.InvariantCulture);
}
