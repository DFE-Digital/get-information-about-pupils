using System.ComponentModel;

namespace DfE.GIAP.Web.Features.MyPupils.Logging;

public enum LogLevel
{
    [Description("Debug")]
    Debug,

    [Description("Info")]
    Info,

    [Description("Error")]
    Error
}
