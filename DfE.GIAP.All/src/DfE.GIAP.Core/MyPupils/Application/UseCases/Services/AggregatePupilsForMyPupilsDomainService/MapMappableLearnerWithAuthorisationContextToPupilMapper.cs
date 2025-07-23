using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Entities;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.Services.AggregatePupilsForMyPupilsDomainService;
internal sealed class MapMappableLearnerWithAuthorisationContextToPupilMapper : IMapper<MappableLearnerWithAuthorisationContext, Pupil>
{
    public Pupil Map(MappableLearnerWithAuthorisationContext input) =>
        new(
            identifier: new PupilId(Guid.NewGuid()), // TODO identifier should be hydrated on MyPupilList and surfaced up so we can uniquely identify a pupil in the list for Remove / Download operations
            pupilType: input.PupilType,
            name: new(input.Learner.Forename, input.Learner.Surname),
            uniquePupilNumber: new UniquePupilNumber(input.Learner.LearnerNumber),
            dateOfBirth: input.Learner.Dob,
            sex: new Sex(input.Learner.Sex),
            localAuthorityCode: new LocalAuthorityCode(int.Parse(input.Learner.LocalAuthority)),
            authorisationContext: input.AuthorisationContext);
}
