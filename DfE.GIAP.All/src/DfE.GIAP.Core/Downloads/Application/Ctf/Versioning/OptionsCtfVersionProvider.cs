using DfE.GIAP.Core.Downloads.Application.Ctf.Models;
using Microsoft.Extensions.Options;

namespace DfE.GIAP.Core.Downloads.Application.Ctf.Versioning;

public class OptionsCtfVersionProvider : ICtfVersionProvider
{
    private readonly CtfOptions _options;

    public OptionsCtfVersionProvider(IOptions<CtfOptions> options)
    {
        _options = options.Value;
        ArgumentNullException.ThrowIfNullOrWhiteSpace(_options.Version);
    }

    public string GetVersion() => _options.Version;
}
