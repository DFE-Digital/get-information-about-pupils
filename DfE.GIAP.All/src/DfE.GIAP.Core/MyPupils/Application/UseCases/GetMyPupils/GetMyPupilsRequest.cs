using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;

namespace DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils;
public record GetMyPupilsRequest : IUseCaseRequest<GetMyPupilsResponse>
{
    public GetMyPupilsRequest(string userId, MyPupilsQueryModel? query = null)
    {
        UserId = userId ?? string.Empty;
        Query = query ?? MyPupilsQueryModel.CreateDefault();
    }

    public string UserId { get; }
    public MyPupilsQueryModel Query { get; }
}
