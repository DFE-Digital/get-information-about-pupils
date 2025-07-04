using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Core.Models.News;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels.Admin
{
    [ExcludeFromCodeCoverage]
    public class AddUpdateNewsViewModel
    {
        public AddUpdateNewsViewModel()
        {
            this.AddUpdateRequest = new UpdateNewsDocumentRequestBody();
        }

        public UpdateNewsDocumentRequestBody AddUpdateRequest { get; set; }


        [Required(ErrorMessage = CommonErrorMessages.TitleLength)]
        public string Title { get; set; }

        public bool TitleInvalid { get; set; }

        [Required(ErrorMessage = CommonErrorMessages.BodyRequired)]
        public string Body { get; set; }

        public bool BodyInvalid { get; set; }
    }
}
