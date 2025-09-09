using DfE.GIAP.Core.Users.Application;

namespace DfE.GIAP.Web.Features.MyPupils.ViewModel;

public interface IMyPupilsViewModelFactory
{
    Task<MyPupilsViewModel> CreateViewModelAsync(UserId userId, MyPupilsErrorViewModel error = null);
}
