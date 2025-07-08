using DfE.GIAP.Common.Enums;
using System.ComponentModel;

namespace DfE.GIAP.Common.Helpers
{
    public static class LogEventActionTypeHelper
    {
        public static string LogEventActionDescription(this LogEventActionType logEventActionType)
        {
            var logEvent = logEventActionType.GetType().GetField(logEventActionType.ToString());

            var customDescription = (DescriptionAttribute[])logEvent.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return customDescription[0].Description;
        }
    }
}
