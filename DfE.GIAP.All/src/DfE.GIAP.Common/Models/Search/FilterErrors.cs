using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
    [ExcludeFromCodeCoverage]
    public class FilterErrors
    {
        public bool SurnameError { get; set; }
        public bool MiddlenameError { get; set; }
        public bool ForenameError { get; set; }
        public bool GenderError { get; set; }
        public bool SexError { get; set; }

        public bool DobErrorEmpty { get; set; }
        public bool DobErrorDayOnly { get; set; }
        public bool DobErrorMonthOnly { get; set; }
        public bool DobErrorNoMonth { get; set; }
        public bool DobError { get; set; }
        public bool DobErrorDayMonthOnly { get; set; }
        public bool YearLimitHigh { get; set; }
        public bool YearLimitLow { get; set; }
        public bool MonthOutOfRange { get; set; }
        public bool DayOutOfRange { get; set; }
        public bool InvalidDob { get; set; }
    }
}
