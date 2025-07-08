using System.ComponentModel;

namespace DfE.GIAP.Common.Enums
{
    public enum LogEventActionType
    {
        [Description("User logged after DSI authentication")]
        UserLoggedIn,

        [Description("User has been accepted the Consent")]
        ConsentPageAccessed,

        [Description("User is downloading PrePreapared file")]
        DownloadPrePreparedFile,
    }
}
