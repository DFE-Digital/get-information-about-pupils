using System.ComponentModel;

namespace DfE.GIAP.Common.Enums
{
    public enum DownloadFileType
    {
        [Description("None")]
        None = 0,
        [Description("CSV")]
        CSV = 1,
        [Description("TAB")]
        TAB = 2,
    }
}
