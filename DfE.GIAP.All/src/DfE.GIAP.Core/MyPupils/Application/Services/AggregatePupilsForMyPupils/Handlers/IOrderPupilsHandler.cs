using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.QueryModel;
using DfE.GIAP.Core.MyPupils.Domain.Entities;

namespace DfE.GIAP.Core.MyPupils.Application.Services.AggregatePupilsForMyPupils.Handlers;
public interface IOrderPupilsHandler
{
    IEnumerable<Pupil> Order(IEnumerable<Pupil> pupils, OrderOptions options);
}
