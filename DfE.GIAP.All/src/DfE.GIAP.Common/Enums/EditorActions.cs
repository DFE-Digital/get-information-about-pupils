using System.ComponentModel;

namespace DfE.GIAP.Common.Enums
{
    public enum EditorActions
    {
        [Description("Add")]
        add,

        [Description("Edit")]
        edit,

        [Description("Discard")]
        discard,

        [Description("Preview")]
        preview,

        [Description("Delete")]
        delete,

        [Description("SaveAsDraft")]
        saveAsDraft
    }
}
