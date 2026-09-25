using DfE.GIAP.Web.Features.MyPupils.Controllers.UpdateForm;

namespace DfE.GIAP.Web.Features.MyPupils.Services.UpsertSelectedPupils;

public interface IUpsertSelectedPupilsIdentifiersPresentationService
{
    Task<List<string>> UpsertAsync(string userId, MyPupilsPupilSelectionsRequestDto? selectionsDto);
}
