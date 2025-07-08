using System;

namespace DfE.GIAP.Domain.Models.Search
{
    public interface IRbac
    {
        DateTime? DOB { get; set; }

        string LearnerNumber { get; set; }

        string LearnerNumberId { get; set; }
    }
}
