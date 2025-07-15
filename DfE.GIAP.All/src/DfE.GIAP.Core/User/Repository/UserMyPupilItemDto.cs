using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Core.User.Repository;
public class UserMyPupilItemDto
{
    public string? PupilId { get; set; }
    // public bool IsMasked { get; set; } // Do we still need this given Masked is evaluated by the Domain.
}
