using DfE.GIAP.Core.Common.Application;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils;
using DfE.GIAP.Web.Features.MyPupils.Handlers.GetPaginatedMyPupils.PresentationHandlers;
using Moq;
using Xunit;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.GetPaginatedMyPupils;
public sealed class GetPaginatedMyPupilsHandlerTests
{
    [Fact]
    public void Test1()
    {
        Mock<IUseCase<GetMyPupilsRequest, GetMyPupilsResponse>> useCaseMock = new();
        Mock<IPupilDtosPresentationHandler> mockHandler = new();
        
        //GetPaginatedMyPupilsHandler handler = new(useCaseMock.Object)
    }
}
