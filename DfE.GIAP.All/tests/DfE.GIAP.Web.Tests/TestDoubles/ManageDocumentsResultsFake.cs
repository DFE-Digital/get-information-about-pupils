using DfE.GIAP.Core.Models.Common;
using DfE.GIAP.Core.Models.Editor;
using DfE.GIAP.Web.ViewModels;
using DfE.GIAP.Web.ViewModels.Admin;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class ManageDocumentsResultsFake
    {
        public ManageDocumentsViewModel GetDocumentDetails()
        {
            return new ManageDocumentsViewModel
            {
                DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1 },
                DocumentData = new CommonResponseBodyViewModel()
                {
                    Title = "testTitle",
                    Body = "testBody",
                    Id = "PlannedMaintenance"
                }
            };
        }

        public ManageDocumentsViewModel GetDocumentDetailsNoID()
        {
            return new ManageDocumentsViewModel
            {
                DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1 },
                DocumentData = new CommonResponseBodyViewModel()
                {
                    Title = "testTitle",
                    Body = "testBody"
                }
            };
        }

        public ManageDocumentsViewModel GetDocumentDetailsWithSelectedNews()
        {
            return new ManageDocumentsViewModel
            {
                DocumentList = new Document() { Id = 1, DocumentId = "TestNewsArticle", DocumentName = "Test News Articles", SortId = 1 },
                DocumentData = new CommonResponseBodyViewModel()
                {
                    Title = "testTitle",
                    Body = "testBody",
                    Id = "PlannedMaintenance"
                },
                SelectedNewsId = "1"
            };
        }

        public CommonResponseBody GetCommonResponseBody()
        {
            return new CommonResponseBody() { Title = "testTitle", Body = "testBody", Id = "PlannedMaintenance" };
        }
    }
}
