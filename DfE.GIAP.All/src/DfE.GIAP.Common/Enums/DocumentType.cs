using System.ComponentModel;

namespace DfE.GIAP.Common.Enums;

public enum DocumentType
{
    [Description("Article")]
    Article,

    [Description("Glossary")]
    Glossary,

    [Description("Terms of Use")]
    TermOfUse,

    [Description("Privacy Notice")]
    PrivacyNotice,

    [Description("Accessibility")]
    Accessibility,

    [Description("Accessibility Report")]
    AccessibilityReport,

    [Description("Frequently Asked Questions")]
    FAQ,

    [Description("Landing")]
    Landing,
}
