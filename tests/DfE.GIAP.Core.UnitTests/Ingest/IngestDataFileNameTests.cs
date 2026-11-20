using DfE.GIAP.Core.Common.Infrastructure.Postgres.Models;
using DfE.GIAP.Core.Ingest;

namespace DfE.GIAP.Core.UnitTests.Ingest;

public class IngestDataFileNameTests
{
    [Theory]
    [InlineData("NPD_Census_2425_KtS_Autumn.txt", IngestDataset.CensusAutumn, "2024/2025", IngestVersion.Unamended)]
    [InlineData("NPD_Census_1819_KTS_Spring.txt", IngestDataset.CensusSpring, "2018/2019", IngestVersion.Unamended)]
    [InlineData("NPD_Census_1920_KtS_Summer_F.csv", IngestDataset.CensusSummer, "2019/2020", IngestVersion.Final)]
    [InlineData("NPD_EYFSP_2018_F_KtS.txt", IngestDataset.Eyfsp, "2017/2018", IngestVersion.Final)]
    [InlineData("NPD_KS1_2019_U_KtS.txt", IngestDataset.Ks1, "2018/2019", IngestVersion.Unamended)]
    [InlineData("NPD_KS2_2025_A_KtS.txt", IngestDataset.Ks2, "2024/2025", IngestVersion.Amended)]
    [InlineData("npd_ks4_2025_final_kts.tsv", IngestDataset.Ks4, "2024/2025", IngestVersion.Final)]
    [InlineData("NPD_MTC_2025_U_KtS.txt", IngestDataset.Mtc, "2024/2025", IngestVersion.Unamended)]
    [InlineData("NPD_PHONICS_2018_F_KtS.txt", IngestDataset.Phonics, "2017/2018", IngestVersion.Final)]
    [InlineData("NPD_pupil-premium_2425_U.csv", IngestDataset.PupilPremium, "2024/2025", IngestVersion.Unamended)]
    [InlineData("NPD_pupil-premium_FFD_CLA_1920_F.txt", IngestDataset.PupilPremium, "2019/2020", IngestVersion.Final)]
    [InlineData("FE_PP_2025.csv", IngestDataset.FurtherEducationPupilPremium, "2024/2025", IngestVersion.Unamended)]
    [InlineData("FE SEN 2025.csv", IngestDataset.FurtherEducationSpecialEducationalNeeds, "2024/2025", IngestVersion.Unamended)]
    [InlineData("GIAP_CLA_KS2_2025.txt", IngestDataset.ChildrenLookedAfterKs2, "2024/2025", IngestVersion.Unamended)]
    [InlineData("GIAP_CLA_KS4_2025_F.csv", IngestDataset.ChildrenLookedAfterKs4, "2024/2025", IngestVersion.Final)]
    public void Parse_ValidName_ReturnsAttributes(string fileName, IngestDataset dataset, string academicYear, IngestVersion ingestVersion)
    {
        IngestDataFileAttributes attributes = IngestDataFileName.Parse(fileName);

        Assert.Equal(new IngestDataFileAttributes(dataset, academicYear, ingestVersion), attributes);
    }

    [Theory]
    [InlineData("NPD_KS2_2025_U_KtS.zip")]
    [InlineData("NPD_KS2_2025_U_KtS.xlsx")]
    [InlineData("NPD_KS2_2025_U_KtS")]
    [InlineData("NPD_2025_U_KtS.txt")]
    [InlineData("NPD_KS2_KS4_2025_U.txt")]
    [InlineData("GIAP_CLA_KS2_KS4_2025.txt")]
    [InlineData("GIAP_CLA_2025.txt")]
    [InlineData("NPD_KS5_2025_U.txt")]
    [InlineData("NPD_Census_2425_KtS.txt")]
    [InlineData("NPD_Census_2425_Autumn_Spring.txt")]
    [InlineData("NPD_Census_2426_Autumn.txt")]
    [InlineData("NPD_pupil-premium_2025_U.txt")]
    [InlineData("FE_2025.csv")]
    [InlineData("NPD_KS2_U_KtS.txt")]
    [InlineData("NPD_KS2_2024_2025_U.txt")]
    [InlineData("NPD_KS2_25_U.txt")]
    [InlineData("NPD_KS2_2025_U_F.txt")]
    public void Parse_InvalidName_Throws(string fileName)
    {
        IngestDataFileNameException ex = Assert.Throws<IngestDataFileNameException>(() => IngestDataFileName.Parse(fileName));

        Assert.Equal(fileName, ex.FileName);
    }

    [Theory]
    [InlineData("KS2 metadata.xlsx", true)]
    [InlineData("Guidance.DOC", true)]
    [InlineData("Guidance.docx", true)]
    [InlineData("NPD_KS2_2025_U_KtS.txt", false)]
    public void IsMetadataDocument(string fileName, bool expected) =>
        Assert.Equal(expected, IngestDataFileName.IsMetadataDocument(fileName));
}
