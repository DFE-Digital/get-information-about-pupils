using DfE.GIAP.Core.Models.Editor;

namespace DfE.GIAP.Service.ManageDocument;

public interface IManageDocumentsService
{
    IList<Document> GetDocumentsList();
}
