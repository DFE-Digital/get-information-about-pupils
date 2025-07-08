using DfE.GIAP.Domain.Models.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.User
{
    [ExcludeFromCodeCoverage]
    public class UserAccess
    {
        public Guid UserId { get; set; }
        public Guid ServiceId { get; set; }
        public Guid OrganisationId { get; set; }
        public IEnumerable<UserRole> Roles { get; set; }
        public IEnumerable<KeyValue> Identifiers { get; set; }

    }
}
