using System.ComponentModel;

namespace DfE.GIAP.Web.Features.MyPupils.Messaging;

public enum MessageLevel
{
    [Description("Debug")]
    Debug,

    [Description("Info")]
    Info,

    [Description("Error")]
    Error
}