using System.ComponentModel;

namespace DfE.GIAP.Core.Models
{
    public enum ActionTypes
    {
        [Description("Archive")]
        Archive = 1,

        [Description("Publish")]
        Publish = 2,

        [Description("Unarchive")]
        Unarchive = 3,

        [Description("Unpublish")]
        Unpublish = 4,

        [Description("Pinned")]
        Pinned = 5,

        [Description("Unpinned")]
        Unpinned = 6,
    }
}
