using System.ComponentModel;

namespace DfE.GIAP.Common.Enums;

public enum DocumentType
{
    [Description("Article")]
    Article,

    [Description("Terms of Use")]
    TermOfUse,

    [Description("Privacy Notice")]
    PrivacyNotice,

    [Description("Landing")]
    Landing,
}
