using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Models.DownloadOutputs;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers.Mappers;

public class NationalPupilToKs1OutputRecordMapper : IMapper<NationalPupil, IEnumerable<KS1Output>>
{
    public IEnumerable<KS1Output> Map(NationalPupil input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.KeyStage1 is null || !input.KeyStage1.Any())
            return Enumerable.Empty<KS1Output>();

        return input.KeyStage1.Select(ks1Entry => new KS1Output
        {
            ACADYR = ks1Entry?.ACADYR,
            PUPILMATCHINGREF = ks1Entry?.PUPILMATCHINGREF,
            KS1_ID = ks1Entry?.KS1_ID,
            UPN = ks1Entry?.UPN,
            SURNAME = ks1Entry?.SURNAME,
            FORENAMES = ks1Entry?.FORENAMES,
            DOB = ks1Entry?.DOB?.ToShortDateString(),
            GENDER = ks1Entry?.GENDER,
            SEX = ks1Entry?.SEX,
            LA = ks1Entry?.LA,
            LA_9Code = ks1Entry?.LA_9Code,
            ESTAB = ks1Entry?.ESTAB,
            LAESTAB = ks1Entry?.LAESTAB,
            URN = ks1Entry?.URN,
            ToE_CODE = ks1Entry?.ToE_CODE,
            MMSCH = ks1Entry?.MMSCH,
            MMSCH2 = ks1Entry?.MMSCH2,
            MSCH = ks1Entry?.MSCH,
            MSCH2 = ks1Entry?.MSCH2,
            MOB1 = ks1Entry?.MOB1,
            MOB2 = ks1Entry?.MOB2,
            DISC_READ = ks1Entry?.DISC_READ,
            DISC_WRIT = ks1Entry?.DISC_WRIT,
            DISC_MAT = ks1Entry?.DISC_MAT,
            DISC_SCI = ks1Entry?.DISC_SCI,
            READ_OUTCOME = ks1Entry?.READ_OUTCOME,
            WRIT_OUTCOME = ks1Entry?.WRIT_OUTCOME,
            MATH_OUTCOME = ks1Entry?.MATH_OUTCOME,
            SCI_OUTCOME = ks1Entry?.SCI_OUTCOME,
            NPDPUB = ks1Entry?.NPDPUB,
            VERSION = ks1Entry?.VERSION
        });
    }
}
