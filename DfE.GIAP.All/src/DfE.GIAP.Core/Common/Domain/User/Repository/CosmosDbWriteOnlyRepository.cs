using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Core.Common.Domain.User.Repository;
internal sealed class CosmosDbWriteOnlyRepository : IUserWriteRepository
{
    public Task SaveAsync(UserAggregateRoot user) => throw new NotImplementedException();
}
