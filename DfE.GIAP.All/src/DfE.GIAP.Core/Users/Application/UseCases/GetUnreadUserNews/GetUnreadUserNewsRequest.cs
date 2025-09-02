using DfE.GIAP.Core.Common.Application;

namespace DfE.GIAP.Core.Users.Application.UseCases.GetUnreadUserNews;
public record GetUnreadUserNewsRequest(string UserId) : IUseCaseRequest<GetUnreadUserNewsResponse>;
