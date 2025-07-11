using System.ComponentModel;

namespace DfE.GIAP.Core.Models;

public enum ActionTypes
{
    [Description("Publish")]
    Publish = 2,

    [Description("Unpublish")]
    Unpublish = 4,

    [Description("Pinned")]
    Pinned = 5,

    [Description("Unpinned")]
    Unpinned = 6,
}
