using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Views.Shared.Components.ReleaseTimetable;

public class SchoolCensusDataViewModel
{
    public required string AcademicYear { get; set; }
    public required bool AutumnCensus { get; set; }
    public required bool SpringCensus { get; set; }
    public required bool SummerCensus { get; set; }
    public required bool PupilPremium { get; set; }
    public required bool DisadvantagePremium16To18 { get; set; }
    public required bool Sen16To18 { get; set; }
}

public class AttainmentDataViewModel
{
    public required string AcademicYear { get; set; }
    public required bool KeyStage1 { get; set; }
    public required bool KeyStage2 { get; set; }
    public required bool KeyStage4 { get; set; }
    public required bool EYFSP { get; set; }
    public required bool Phonics { get; set; }
    public required bool PhonicsAutumnY2 { get; set; }
    public required bool MTC { get; set; }
}

public class ReleaseTimetableViewModel
{
    public required List<SchoolCensusDataViewModel> SchoolCensusData { get; set; }
    public required List<AttainmentDataViewModel> AttainmentData { get; set; }
}



public class ReleaseTimetableViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        ReleaseTimetableViewModel model = new()
        {
            SchoolCensusData = new List<SchoolCensusDataViewModel>
            {
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2025-26",
                    AutumnCensus = false,
                    SpringCensus = false,
                    SummerCensus = false,
                    PupilPremium = true,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2024-25",
                    AutumnCensus = true,
                    SpringCensus = false,
                    SummerCensus = false,
                    PupilPremium = true,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2023-24",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = true,
                    DisadvantagePremium16To18 = true,
                    Sen16To18 = true
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2022-23",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = true,
                    DisadvantagePremium16To18 = true,
                    Sen16To18 = true
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2021-22",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = true,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2020-21",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = true,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2019-20",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = false,
                    PupilPremium = false,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2018-19",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = false,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2017-18",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = false,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2016-17",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = false,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
                new SchoolCensusDataViewModel
                {
                    AcademicYear = "2015-16",
                    AutumnCensus = true,
                    SpringCensus = true,
                    SummerCensus = true,
                    PupilPremium = false,
                    DisadvantagePremium16To18 = false,
                    Sen16To18 = false
                },
            },
            AttainmentData = new List<AttainmentDataViewModel>
            {
                new AttainmentDataViewModel
                {
                    AcademicYear = "2023-24",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = false,
                    PhonicsAutumnY2 = true,
                    MTC = true
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2022-23",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = false,
                    PhonicsAutumnY2 = true,
                    MTC = true
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2021-22",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = true,
                    MTC = true
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2020-21",
                    KeyStage1 = false,
                    KeyStage2 = false,
                    KeyStage4 = true,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                 new AttainmentDataViewModel
                {
                    AcademicYear = "2019-20",
                    KeyStage1 = false,
                    KeyStage2 = false,
                    KeyStage4 = true,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2018-19",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2017-18",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2016-17",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2015-16",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2014-15",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = true,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2013-14",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2012-13",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = true,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2011-12",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = true,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2010-11",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2009-10",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2008-09",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                 new AttainmentDataViewModel
                {
                    AcademicYear = "2007-08",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2006-07",
                    KeyStage1 = true,
                    KeyStage2 = true,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
                new AttainmentDataViewModel
                {
                    AcademicYear = "2005-06",
                    KeyStage1 = true,
                    KeyStage2 = false,
                    KeyStage4 = false,
                    EYFSP = false,
                    Phonics = false,
                    PhonicsAutumnY2 = false,
                    MTC = false
                },
            }
        };

        return View(model);
    }
}
