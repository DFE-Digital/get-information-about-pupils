namespace DfE.GIAP.Web.Features.MyPupils.GetMyPupils;

public record MyPupilsResponse
{
    public MyPupilsResponse(MyPupilsPresentationModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        MyPupils = model;
    }

    public MyPupilsPresentationModel MyPupils { get; }
}
