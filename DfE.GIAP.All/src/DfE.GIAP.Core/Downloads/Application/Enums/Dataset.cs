using System.ComponentModel;

namespace DfE.GIAP.Core.Downloads.Application.Enums;

public enum Dataset
{
    // TODO: Census dataset names should be Pascal, but FA expects underscores, remove underscores when possible
    [Description("Autumn Census")]
    Census_Autumn,
    [Description("Spring Census")]
    Census_Spring,
    [Description("Summer Census")]
    Census_Summer,
    [Description("EYFSP")]
    EYFSP,
    [Description("Key Stage 1")]
    KS1,
    [Description("Key Stage 2")]
    KS2,
    [Description("Key Stage 4")]
    KS4,
    [Description("Phonics")]
    Phonics,
    [Description("Pupil Premium")]
    PP,
    [Description("MTC")]
    MTC,
    [Description("Special Educational Needs")]
    SEN
}
