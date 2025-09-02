using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class MyPupilsTestDoubles
{
    public static Core.MyPupils.Application.Repositories.MyPupils Default()
        => new(UniquePupilNumbers.Create(
                    uniquePupilNumbers: UniquePupilNumberTestDoubles.Generate(count: 10)));
}
