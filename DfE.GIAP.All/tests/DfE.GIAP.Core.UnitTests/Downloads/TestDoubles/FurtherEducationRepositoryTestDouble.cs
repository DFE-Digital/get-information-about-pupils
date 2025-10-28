using DfE.GIAP.Core.Downloads.Application.Models;
using DfE.GIAP.Core.Downloads.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.Downloads.TestDoubles;

public static class FurtherEducationRepositoryTestDouble
{
    public static IFurtherEducationRepository WithPupils(IEnumerable<FurtherEducationPupil> pupils) =>
        new StubRepository(pupils);

    private sealed class StubRepository : IFurtherEducationRepository
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
