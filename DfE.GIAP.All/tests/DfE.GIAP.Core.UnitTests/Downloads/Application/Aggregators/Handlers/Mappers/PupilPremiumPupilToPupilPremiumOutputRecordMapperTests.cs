using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class PupilPremiumPupilToPupilPremiumOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_MapsSimplePropertiesCorrectly()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create();

        PupilPremiumOutputRecord result = mapper.Map(pupil);

        Assert.Equal(pupil.UniquePupilNumber, result.UniquePupilNumber);
        Assert.Equal(pupil.Forename, result.Forename);
        Assert.Equal(pupil.Surname, result.Surname);
        Assert.Equal(pupil.Sex, result.Sex);
        Assert.Equal(pupil.DOB.ToShortDateString(), result.DOB);
    }

    [Fact]
    public void Map_MapsPupilPremiumEntry_WhenEntryExists()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        PupilPremiumOutputRecord result = mapper.Map(pupil);

        PupilPremiumEntry entry = pupil.PupilPremium[0];

        Assert.Equal(entry.NCYear, result.NCYear);
        Assert.Equal(entry.DeprivationPupilPremium, result.DeprivationPupilPremium);
        Assert.Equal(entry.ServiceChildPremium, result.ServiceChildPremium);
        Assert.Equal(entry.AdoptedfromCarePremium, result.AdoptedfromCarePremium);
        Assert.Equal(entry.LookedAfterPremium, result.LookedAfterPremium);
        Assert.Equal(entry.PupilPremiumFTE, result.PupilPremiumFTE);
        Assert.Equal(entry.PupilPremiumCashAmount, result.PupilPremiumCashAmount);
        Assert.Equal(entry.PupilPremiumFYStartDate, result.PupilPremiumFYStartDate);
        Assert.Equal(entry.PupilPremiumFYEndDate, result.PupilPremiumFYEndDate);
        Assert.Equal(entry.LastFSM, result.LastFSM);
    }

    [Fact]
    public void Map_SetsPpFieldsToNull_WhenNoPpEntryExists()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: false);

        PupilPremiumOutputRecord result = mapper.Map(pupil);

        Assert.Null(result.NCYear);
        Assert.Null(result.DeprivationPupilPremium);
        Assert.Null(result.ServiceChildPremium);
        Assert.Null(result.AdoptedfromCarePremium);
        Assert.Null(result.LookedAfterPremium);
        Assert.Null(result.PupilPremiumFTE);
        Assert.Null(result.PupilPremiumCashAmount);
        Assert.Null(result.PupilPremiumFYStartDate);
        Assert.Null(result.PupilPremiumFYEndDate);
        Assert.Null(result.LastFSM);
    }

    [Fact]
    public void Map_SetsPpFieldsToNull_WhenPpListIsNull()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: false);

        PupilPremiumOutputRecord result = mapper.Map(pupil);

        Assert.Null(result.NCYear);
        Assert.Null(result.DeprivationPupilPremium);
        Assert.Null(result.ServiceChildPremium);
        Assert.Null(result.AdoptedfromCarePremium);
        Assert.Null(result.LookedAfterPremium);
        Assert.Null(result.PupilPremiumFTE);
        Assert.Null(result.PupilPremiumCashAmount);
        Assert.Null(result.PupilPremiumFYStartDate);
        Assert.Null(result.PupilPremiumFYEndDate);
        Assert.Null(result.LastFSM);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInPupilPremiumEntry()
    {
        PupilPremiumPupilToPupilPremiumOutputRecordMapper mapper = new();
        PupilPremiumPupil pupil = PupilPremiumPupilTestDouble.Create(includePupilPremium: true);

        PupilPremiumOutputRecord result = mapper.Map(pupil);

        Assert.Null(result.GetType().GetProperty("MODSERVICE"));
        Assert.Null(result.GetType().GetProperty("CENSUSSERVICEEVER6"));
    }
}

