using DfE.GIAP.Core.Downloads.Application.Ctf;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Downloads.Application.Aggregators.Handlers;

public class CtfHeaderHandler : ICtfHeaderHandler
{
    private readonly ICtfVersionProvider _versionProvider;
    public const string DescriptorEstablishment =
        "This attainment data was obtained via the Searchable pupil data option of Get Information About Pupils";
    public const string DescriptorNonEstablishment =
        "This attainment data was obtained via the Get Information About Pupils school site";

    public CtfHeaderHandler(ICtfVersionProvider versionProvider)
    {
        ArgumentNullException.ThrowIfNull(versionProvider);
        _versionProvider = versionProvider;
    }

    public CtfHeader Build(ICtfHeaderContext context)
    {
        DateTime now = DateTime.UtcNow;

        return new CtfHeader
        {
            DocumentName = "Common Transfer File",
            CtfVersion = _versionProvider.GetVersion(), // Changes every september(ish)
            DateTime = now,
            DocumentQualifier = "partial",
            DataDescriptor = context.IsEstablishment
                ? DescriptorEstablishment
                : DescriptorNonEstablishment,
            SupplierId = "GIAP",

            SourceSchool = new CtfSchoolInfo
            {
                LEA = "XXX",
                Estab = "XXXX",
                SchoolName = "Get Information About Pupils",
                AcademicYear = CalculateAcademicYear(now)
            },

            DestSchool = new CtfSchoolInfo
            {
                LEA = context.IsEstablishment ? context.LocalAuthorityNumber : "XXX",
                Estab = context.IsEstablishment ? context.EstablishedNumber : "XXXX"
            }
        };
    }

    private string CalculateAcademicYear(DateTime now)
    {
        int year = now.Month >= 9 ? now.Year : now.Year - 1;
        return year.ToString();
    }
}

public interface ICtfVersionProvider
{
    string GetVersion();
}

public class OptionsCtfVersionProvider : ICtfVersionProvider
{
    private readonly CtfOptions _options;

    public OptionsCtfVersionProvider(IOptions<CtfOptions> options)
    {
        _options = options.Value;
        ArgumentException.ThrowIfNullOrWhiteSpace(_options.Version);
    }
    public string GetVersion() => _options.Version;
}
