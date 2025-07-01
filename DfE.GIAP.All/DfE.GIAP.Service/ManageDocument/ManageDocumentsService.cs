using DfE.GIAP.Common.Enums;
using DfE.GIAP.Core.Models.Editor;
using System.ComponentModel;
using System.Reflection;

namespace DfE.GIAP.Service.ManageDocument;

public class ManageDocumentsService : IManageDocumentsService
{
    public ManageDocumentsService() { }

    public IList<Document> GetDocumentsList()
    {
        IList<Document> enumValList = new List<Document>();
        int counter = 1;
        foreach (var e in Enum.GetValues(typeof(DocumentType)))
        {
            FieldInfo fieldInfo = e.GetType().GetField(e.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            Document document = new()
            {
                Id = counter,
                DocumentId = e.ToString(),
                DocumentName = attributes[0].Description,
                SortId = counter,
                IsEnabled = true
            };
            enumValList.Add(document);
            counter++;
        }
        return enumValList.OrderBy(x => x.DocumentName).ToList();
    }
}
