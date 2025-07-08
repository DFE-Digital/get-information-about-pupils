using System.ComponentModel;

namespace DfE.GIAP.Core.Models.Common
{
    public enum UpdateDocumentType
    {
        //Started at 6 to match enum in kts project.

        [Description("News Publications")]
        NewsPublications = 6,

        [Description("News Articles")]
        NewsArticles = 7,

        [Description("News Maintenace")]
        NewsMaintenance = 8,

        [Description("User Profile")]
        UserProfile = 10,

        [Description("Stage Assessments SectionRules")]
        StageAssessmentsSectionRules = 11,

        [Description("CTF FileHeader")]
        CTFFileHeader = 12,

        [Description("Content")]
        Content = 20
    }
}
