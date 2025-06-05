﻿using System.ComponentModel;

namespace DfE.GIAP.Common.Enums
{
    public enum DocumentType
    {
        [Description("Article")]
        Article,

        [Description("Publication Schedule")]
        PublicationSchedule,

        [Description("Planned Maintenance")]
        PlannedMaintenance,

        [Description("Archived Article")]
        ArchivedNews,

        [Description("Consent")]
        Consent,

        [Description("Cookie Preference")]
        CookiePreferences,

        [Description("Cookies Measure Website")]
        CookiesMeasureWebsite,

        [Description("Cookies Help")]
        CookiesHelp,

        [Description("Cookies Necessary")]
        CookiesNecessary,

        [Description("Cookie Details")]
        CookieDetails,

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
}
