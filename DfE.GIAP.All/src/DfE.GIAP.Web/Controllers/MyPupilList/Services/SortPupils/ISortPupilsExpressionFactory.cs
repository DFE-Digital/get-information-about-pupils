using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Response;
using System.Linq.Expressions;

namespace DfE.GIAP.Web.Controllers.MyPupilList.Services.SortExpressions;

public interface ISortPupilsExpressionFactory
{
    Expression<Func<PupilDto, IComparable>> Create(string sortKey);
}
