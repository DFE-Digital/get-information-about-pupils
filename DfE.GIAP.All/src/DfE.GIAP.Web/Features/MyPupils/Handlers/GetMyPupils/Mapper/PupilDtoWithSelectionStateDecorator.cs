using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;

namespace DfE.GIAP.Web.Features.MyPupils.Handlers.GetMyPupils.Mapper;

public record PupilDtoWithSelectionStateDecorator(PupilDto Pupil, bool IsSelected)
{
    public static PupilDtoWithSelectionStateDecorator Create(PupilDto pupil, bool isSelected)
    {
        ArgumentNullException.ThrowIfNull(pupil);
        return new(pupil, isSelected);
    }
}
