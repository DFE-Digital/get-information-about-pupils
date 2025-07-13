using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DfE.GIAP.Core.Common.Domain;

namespace DfE.GIAP.Core.MyPupils.Domain;
//public sealed class MyPupils : AggregateRoot<>
//{
//}
// TODO currently MyPupils is accessible within a single user scope. via the users UserProfile.
// Lists do not appear to be shared or shareable between consumers in an organisation, unless they share a "UserAccount?"

// It appears to me the AggregateRoot here is UserProfile. Access being contained within it to MyPupils as an entity,
// This may evolve in the future if lists become shareable, if users can hold multiple lists?
