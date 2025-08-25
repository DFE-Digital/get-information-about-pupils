using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Web.Controllers.MyPupilList.PupilSelectionState.Provider;
using Moq;

namespace DfE.GIAP.Web.Tests.Controllers.MyPupilList.TestDoubles;
internal static class PupilSelectionStateProviderTestDoubles
{
    internal static Mock<IPupilSelectionStateProvider> Default() => new();
}
