using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.Common.Application.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles.Learner;
public static class LocalAuthorityCodeTestDoubles
{
    public static LocalAuthorityCode Stub(int? code = null) => new(code ?? 100);
}
