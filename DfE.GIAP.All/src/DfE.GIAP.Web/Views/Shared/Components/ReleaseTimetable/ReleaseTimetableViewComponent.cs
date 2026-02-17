using Microsoft.AspNetCore.Mvc;

namespace DfE.GIAP.Web.Views.Shared.Components.ReleaseTimetable;

public enum SchoolCensusType
{
    AutumnCensus,
    SpringCensus,
    SummerCensus,
    PupilPremium,
    DisadvantagePremium16To18,
    Sen16To18
}

public enum AttainmentType
{
    KeyStage1,
    KeyStage2,
    KeyStage4,
    EYFSP,
    Phonics,
    PhonicsAutumnY2,
    MTC
}


public class SchoolCensusYearViewModel
{
    public required string AcademicYear { get; set; }
    public HashSet<SchoolCensusType> Available { get; set; } = new();
}

public class AttainmentYearViewModel
{
    public required string AcademicYear { get; set; }
    public HashSet<AttainmentType> Available { get; set; } = new();
}

public class ReleaseTimetableViewModel
{
    public required List<SchoolCensusYearViewModel> SchoolCensusData { get; set; }
    public required List<AttainmentYearViewModel> AttainmentData { get; set; }
}


public class ReleaseTimetableViewComponent : ViewComponent
{
    public const string Name = "ReleaseTimetable";

    public IViewComponentResult Invoke()
    {
        ReleaseTimetableViewModel model = new()
        {
            SchoolCensusData =
            [
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2026-27",
                    Available = new()
                    {
                        // Autumn, Spring, Summer, Disadvantage, SEN are false in source
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2025-26",
                    Available = new()
                    {
                        SchoolCensusType.PupilPremium,
                        SchoolCensusType.AutumnCensus
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2024-25",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus,
                        SchoolCensusType.PupilPremium,
                        SchoolCensusType.DisadvantagePremium16To18,
                        SchoolCensusType.Sen16To18
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2023-24",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus,
                        SchoolCensusType.PupilPremium,
                        SchoolCensusType.DisadvantagePremium16To18,
                        SchoolCensusType.Sen16To18
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2022-23",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus,
                        SchoolCensusType.PupilPremium,
                        SchoolCensusType.DisadvantagePremium16To18,
                        SchoolCensusType.Sen16To18
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2021-22",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus,
                        SchoolCensusType.PupilPremium
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2020-21",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus,
                        SchoolCensusType.PupilPremium
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2019-20",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2018-19",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2017-18",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2016-17",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus
                    }
                },
                new SchoolCensusYearViewModel
                {
                    AcademicYear = "2015-16",
                    Available = new()
                    {
                        SchoolCensusType.AutumnCensus,
                        SchoolCensusType.SpringCensus,
                        SchoolCensusType.SummerCensus
                    }
                },
            ],

            AttainmentData =
            [
                new AttainmentYearViewModel
                {
                    AcademicYear = "2024-25",
                    Available = new()
                    {
                        AttainmentType.EYFSP,
                        AttainmentType.MTC,
                        AttainmentType.KeyStage2,
                        AttainmentType.Phonics,
                        AttainmentType.PhonicsAutumnY2
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2023-24",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics,
                        AttainmentType.PhonicsAutumnY2,
                        AttainmentType.MTC
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2022-23",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.PhonicsAutumnY2,
                        AttainmentType.MTC
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2021-22",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics,
                        AttainmentType.PhonicsAutumnY2,
                        AttainmentType.MTC
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2020-21",
                    Available = new()
                    {
                        AttainmentType.KeyStage4
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2019-20",
                    Available = new()
                    {
                        AttainmentType.KeyStage4
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2018-19",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2017-18",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2016-17",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2015-16",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2014-15",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.KeyStage4,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2013-14",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2012-13",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.EYFSP,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2011-12",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2,
                        AttainmentType.Phonics
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2010-11",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2009-10",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2008-09",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2007-08",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2006-07",
                    Available = new()
                    {
                        AttainmentType.KeyStage1,
                        AttainmentType.KeyStage2
                    }
                },
                new AttainmentYearViewModel
                {
                    AcademicYear = "2005-06",
                    Available = new()
                    {
                        AttainmentType.KeyStage1
                    }
                },
            ]
        };

        return View(model);
    }
}
