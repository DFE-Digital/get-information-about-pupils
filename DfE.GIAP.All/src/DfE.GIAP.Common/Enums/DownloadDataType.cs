using System.ComponentModel;

namespace DfE.GIAP.Common.Enums
{
    public enum DownloadDataType
    {
        [Description("EYFSP")]
        EYFSP = 0,
        [Description("Key Stage 1")]
        KS1 = 1,
        [Description("Key Stage 2")]
        KS2 = 2,
        [Description("Key Stage 4")]
        KS4 = 3,
        [Description("Phonics")]
        Phonics = 4,
        [Description("MTC")]
        MTC = 5,
    }
}
