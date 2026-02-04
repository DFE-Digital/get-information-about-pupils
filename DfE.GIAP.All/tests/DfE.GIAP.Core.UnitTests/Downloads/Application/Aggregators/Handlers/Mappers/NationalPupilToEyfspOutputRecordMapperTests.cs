using System.Globalization;
using DfE.GIAP.Core.Common.Application.Helpers;
using DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;
using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;
using DfE.GIAP.Core.Downloads.Application.Models.Entries;
using DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

namespace DfE.GIAP.Core.UnitTests.Downloads.Application.Aggregators.Handlers.Mappers;

public sealed class NationalPupilToEyfspOutputRecordMapperTests
{
    [Fact]
    public void Map_ThrowsArgumentNullException_WhenInputIsNull()
    {
        NationalPupilToEyfspOutputRecordMapper mapper =
            new NationalPupilToEyfspOutputRecordMapper();

        Action act = () => mapper.Map(null!);

        Assert.Throws<ArgumentNullException>(act);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenEyfspListIsNull()
    {
        NationalPupilToEyfspOutputRecordMapper mapper =
            new NationalPupilToEyfspOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeEyfsp: false);
        pupil.EarlyYearsFoundationStageProfile = null!;

        IEnumerable<EYFSPOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_ReturnsEmpty_WhenEyfspListIsEmpty()
    {
        NationalPupilToEyfspOutputRecordMapper mapper =
            new NationalPupilToEyfspOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeEyfsp: false);

        IEnumerable<EYFSPOutputRecord> result = mapper.Map(pupil);

        Assert.Empty(result);
    }

    [Fact]
    public void Map_MapsAllEyfspEntries()
    {
        NationalPupilToEyfspOutputRecordMapper mapper =
            new NationalPupilToEyfspOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeEyfsp: true);

        IEnumerable<EYFSPOutputRecord> result = mapper.Map(pupil);

        Assert.Equal(pupil.EarlyYearsFoundationStageProfile!.Count, result.Count());
    }

    [Fact]
    public void Map_MapsEyfspEntryFieldsCorrectly()
    {
        NationalPupilToEyfspOutputRecordMapper mapper =
            new NationalPupilToEyfspOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeEyfsp: true);

        EarlyYearsFoundationStageProfileEntry entry =
            pupil.EarlyYearsFoundationStageProfile!.First();

        EYFSPOutputRecord mapped = mapper.Map(pupil).First();

        Assert.Equal(entry.PUPILMATCHINGREF, mapped.FSP_PupilMatchingRef);
        Assert.Equal(entry.ACADYR, mapped.FSP_ACADYR);
        Assert.Equal(entry.UPN, mapped.FSP_UPN);
        Assert.Equal(entry.SURNAME, mapped.FSP_SURNAME);
        Assert.Equal(entry.FORENAME, mapped.FSP_FORENAME);
        Assert.Equal(entry.DOB?.ToString(DateFormatting.StandardDateFormat, CultureInfo.InvariantCulture), mapped.FSP_DOB);
        Assert.Equal(entry.SEX, mapped.FSP_SEX);
        Assert.Equal(entry.MTH_ENTRY, mapped.FSP_MTH_ENTRY);
        Assert.Equal(entry.LA, mapped.FSP_LA);
        Assert.Equal(entry.LA_9CODE, mapped.FSP_LA_9Code);
        Assert.Equal(entry.ESTAB, mapped.FSP_ESTAB);
        Assert.Equal(entry.LAESTAB, mapped.FSP_LAESTAB);
        Assert.Equal(entry.URN, mapped.FSP_URN);
        Assert.Equal(entry.NFTYPE, mapped.FSP_NFTYPE);
        Assert.Equal(entry.NPDPUB, mapped.FSP_NPDPUB);
        Assert.Equal(entry.LLSOA11, mapped.FSP_LLSOA11);
        Assert.Equal(entry.IDACISCORE, mapped.FSP_IDACISCORE);
        Assert.Equal(entry.IDACIRANK, mapped.FSP_IDACIRANK);
        Assert.Equal(entry.COM_E01, mapped.FSP_COM_E01);
        Assert.Equal(entry.COM_E02, mapped.FSP_COM_E02);
        Assert.Equal(entry.PSE_E03, mapped.FSP_PSE_E03);
        Assert.Equal(entry.PSE_E04, mapped.FSP_PSE_E04);
        Assert.Equal(entry.PSE_E05, mapped.FSP_PSE_E05);
        Assert.Equal(entry.PHY_E06, mapped.FSP_PHY_E06);
        Assert.Equal(entry.PHY_E07, mapped.FSP_PHY_E07);
        Assert.Equal(entry.LIT_E08, mapped.FSP_LIT_E08);
        Assert.Equal(entry.LIT_E09, mapped.FSP_LIT_E09);
        Assert.Equal(entry.LIT_E10, mapped.FSP_LIT_E10);
        Assert.Equal(entry.MAT_E11, mapped.FSP_MAT_E11);
        Assert.Equal(entry.MAT_E12, mapped.FSP_MAT_E12);
        Assert.Equal(entry.UTW_E13, mapped.FSP_UTW_E13);
        Assert.Equal(entry.UTW_E14, mapped.FSP_UTW_E14);
        Assert.Equal(entry.UTW_E15, mapped.FSP_UTW_E15);
        Assert.Equal(entry.EXP_E16, mapped.FSP_EXP_E16);
        Assert.Equal(entry.EXP_E17, mapped.FSP_EXP_E17);
        Assert.Equal(entry.AGE, mapped.FSP_AGE);
        Assert.Equal(entry.GLD, mapped.FSP_GLD);
        Assert.Equal(entry.VERSION, mapped.FSP_VERSION);
    }

    [Fact]
    public void Map_HandlesNullEntriesInsideList()
    {
        NationalPupilToEyfspOutputRecordMapper mapper =
            new NationalPupilToEyfspOutputRecordMapper();

        NationalPupil pupil = NationalPupilTestDoubles.Create(includeEyfsp: true);

        pupil.EarlyYearsFoundationStageProfile!.Insert(0, null!);

        List<EYFSPOutputRecord> result = mapper.Map(pupil).ToList();

        Assert.Equal(pupil.EarlyYearsFoundationStageProfile.Count, result.Count);
        Assert.Null(result[0].FSP_PupilMatchingRef);
    }

    [Fact]
    public void Map_IgnoresUnmappedFieldsInEyfspEntry()
    {
        // These exist on the entry but not on the output
        Assert.Null(typeof(EYFSPOutputRecord).GetProperty("COM_G01"));
        Assert.Null(typeof(EYFSPOutputRecord).GetProperty("EXP_G16"));
        Assert.Null(typeof(EYFSPOutputRecord).GetProperty("FSP_IMD_2010"));
        Assert.Null(typeof(EYFSPOutputRecord).GetProperty("FSP_PCON"));
    }
}
