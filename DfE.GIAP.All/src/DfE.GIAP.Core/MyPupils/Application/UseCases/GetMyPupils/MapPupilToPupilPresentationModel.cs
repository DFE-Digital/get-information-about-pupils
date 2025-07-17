using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
internal sealed class MapPupilToPupilPresentationModel : IMapper<Pupil, PupilItemPresentationModel>
{
    public PupilItemPresentationModel Map(Pupil input)
    {
        return new(
            input.Identifier,
            input.UniquePupilNumber,
            input.DateOfBirth);
    }
}
