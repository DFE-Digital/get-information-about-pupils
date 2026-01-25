using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public sealed class NationalPupilRepositoryReadOnlyTestDouble
{
    public static INationalPupilDownloadDatasetReadOnlyRepository WithPupils(IEnumerable<NationalPupil> pupils) =>
       new StubRepository(pupils);

    private sealed class StubRepository : INationalPupilDownloadDatasetReadOnlyRepository
    {
        private readonly IEnumerable<NationalPupil> _pupils;

        public StubRepository(IEnumerable<NationalPupil> pupils)
        {
            _pupils = pupils;
        }

        public Task<IEnumerable<NationalPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds)
        {
            return Task.FromResult(_pupils);
        }
    }
}
