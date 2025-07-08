using System;
using System.Globalization;

namespace DfE.GIAP.Common.Helpers
{
    public static class DateTimeHelper
    {
        public static string ConvertDateTimeToString(DateTime? date)
        {
            if (date is null)
            {
                return null;
            }

            string dateFormated;
            try
            {
                var getDateString = $"{date?.Month}/{date?.Day}/{date?.Year}";
                dateFormated = DateTime.ParseExact(string.Format("{0:MM/dd/yyyy}", date), "MM/dd/yyyy", CultureInfo.InvariantCulture).ToString("dd/M/yyyy");
            }
            catch (Exception e)
            {
                var dateError = e.Message;
                return string.Empty;
            }
            return dateFormated;
        }
    }
}
