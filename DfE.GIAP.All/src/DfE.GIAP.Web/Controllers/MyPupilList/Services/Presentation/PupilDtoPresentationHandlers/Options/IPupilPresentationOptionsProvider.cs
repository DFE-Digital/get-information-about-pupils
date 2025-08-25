namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.Presentation.PupilDtoPresentationHandlers.Options;

public interface IPupilsPresentationOptionsProvider
{
    PupilsPresentationOptions GetOptions();
    void SetOptions(PupilsPresentationOptions options);
}
