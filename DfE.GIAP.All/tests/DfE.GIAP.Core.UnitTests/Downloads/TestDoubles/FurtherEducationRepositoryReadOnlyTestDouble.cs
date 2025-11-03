using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class FurtherEducationRepositoryReadOnlyTestDouble
{
    public static IFurtherEducationReadOnlyRepository WithPupils(IEnumerable<FurtherEducationPupil> pupils) =>
        new StubRepository(pupils);

    private sealed class StubRepository : IFurtherEducationReadOnlyRepository
    {
        private readonly IEnumerable<FurtherEducationPupil> _pupils;

        public StubRepository(IEnumerable<FurtherEducationPupil> pupils)
        {
            _pupils = pupils;
        }

        public Task<IEnumerable<FurtherEducationPupil>> GetPupilsByIdsAsync(IEnumerable<string> pupilIds)
        {
            return Task.FromResult(_pupils);
        }
    }
}
