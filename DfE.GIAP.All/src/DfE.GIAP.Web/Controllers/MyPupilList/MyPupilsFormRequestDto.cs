using DfE.GIAP.Web.Controllers.MyPupilList.ViewModel;

namespace DfE.GIAP.Web.Controllers.MyPupilList;

public sealed class MyPupilsFormRequestDto
{
    public bool SelectAll { get; set; } = false;
    public List<string> SelectedPupils { get; set; } = [];
    public MyPupilsErrorModel? Error { get; set; } = null;
}
