using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.MyPupils.Application.UseCases.GetMyPupils.Request;
using DfE.GIAP.Core.MyPupils.Domain.ValueObjects;

namespace DfE.GIAP.SharedTests.TestDoubles;
public static class AuthorisationContextTestDoubles
{
    public static IAuthorisationContext Default() => new StubAuthorisationContext(Guid.NewGuid().ToString(), false, 0, 0);

    public static IAuthorisationContext WithUser(UserId userId) => new StubAuthorisationContext(userId.Value, false, 0, 0);

    internal sealed class StubAuthorisationContext : IAuthorisationContext
    {
        public StubAuthorisationContext(
            string userId,
            bool isAdmin,
            int lowAge,
            int highAge)
        {
            UserId = userId.ToString();
            IsAdministrator = isAdmin;
            LowAge = lowAge;
            HighAge = highAge;
        }

        public string UserId { get; }

        public int LowAge { get; }

        public int HighAge { get; }

        public bool IsAdministrator { get; }
    }
}


