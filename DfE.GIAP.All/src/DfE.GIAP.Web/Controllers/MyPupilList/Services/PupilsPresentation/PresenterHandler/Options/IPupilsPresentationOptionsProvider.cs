namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

public interface IPupilsPresentationOptionsProvider
{
    PupilsPresentationOptions GetOptions();
    void SetOptions(PupilsPresentationOptions options);
}
