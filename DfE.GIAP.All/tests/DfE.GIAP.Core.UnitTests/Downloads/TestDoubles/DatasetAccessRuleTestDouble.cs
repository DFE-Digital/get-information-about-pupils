using DfE.GIAP.Core.Downloads.Application.Datasets.Access;
using DfE.GIAP.Core.Downloads.Application.Datasets.Access.Rules;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public sealed class DatasetAccessRuleTestDouble : IDatasetAccessRule
{
    private readonly bool _canDownload;

    public DatasetAccessRuleTestDouble(bool canDownload)
    {
        _canDownload = canDownload;
    }

    public bool CanDownload(IAuthorisationContext context) => _canDownload;
}
