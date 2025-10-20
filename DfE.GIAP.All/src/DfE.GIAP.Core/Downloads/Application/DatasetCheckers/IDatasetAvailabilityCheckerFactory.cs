using DfE.GIAP.Core.Downloads.Application.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace DfE.GIAP.Core.Downloads.Application.DatasetCheckers;

public interface IDatasetAvailabilityCheckerFactory
{
    IDatasetAvailabilityChecker GetDatasetChecker(DownloadType type);
}

public class DatasetAvailabilityCheckerFactory : IDatasetAvailabilityCheckerFactory
{
    private readonly IDictionary<DownloadType, IDatasetAvailabilityChecker> _checkers;

    public DatasetAvailabilityCheckerFactory(IEnumerable<IDatasetAvailabilityChecker> checkers)
    {
        // Build a dictionary from the available checkers
        _checkers = checkers.ToDictionary(
            checker => checker.SupportedDownloadType,
            checker => checker);
    }

    public IDatasetAvailabilityChecker GetDatasetChecker(DownloadType type)
    {
        if (_checkers.TryGetValue(type, out IDatasetAvailabilityChecker? checker))
            return checker;

        throw new NotSupportedException($"No dataset checker registered for DownloadType '{type}'");
    }
}
