using System.ComponentModel;

namespace DfE.GIAP.Core.Downloads.Application.Enums;

public enum Datasets
{
    [Description("Autumn Census")]
    CensusAutumn,
    [Description("Spring Census")]
    CensusSpring,
    [Description("Summer Census")]
    CensusSummer,
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
    [Description("Pupil  Premium")]
    PupilPremium,
    [Description("MTC")]
    MTC,
    [Description("Special Educational Needs")]
    SEN
}
