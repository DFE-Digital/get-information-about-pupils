using System;

namespace DfE.GIAP.Web.Features.Search.LegacyModels;

public interface IRbac
{
    DateTime? DOB { get; set; }

    string LearnerNumber { get; set; }

    string LearnerNumberId { get; set; }
}
