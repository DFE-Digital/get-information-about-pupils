using DfE.GIAP.Core.Users.Application;
using DfE.GIAP.Web.Features.MyPupils.Routes;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public interface IMyPupilsViewModelFactory
{
    Task<MyPupilsViewModel> CreateViewModelAsync(
        string userId,
        MyPupilsViewModelContext context);
}
