namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.PupilsPresentation.PresenterHandler.Options;

public interface IPresentPupilOptionsProvider
{
    PresentPupilsOptions GetOptions();
    void SetOptions(PresentPupilsOptions options);
}
