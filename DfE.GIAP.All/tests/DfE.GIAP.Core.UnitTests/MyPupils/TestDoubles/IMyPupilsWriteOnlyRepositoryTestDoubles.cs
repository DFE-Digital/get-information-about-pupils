using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.MyPupils.Application.Repositories;

namespace DfE.GIAP.Core.UnitTests.MyPupils.TestDoubles;
internal static class IMyPupilsWriteOnlyRepositoryTestDoubles
{
    internal static Mock<IMyPupilsWriteOnlyRepository> Default() => new();
}
