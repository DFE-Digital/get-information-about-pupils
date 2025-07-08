using DfE.GIAP.Core.Models.Common;

namespace DfE.GIAP.Web.ViewModels.Helper;

public static class CommonResponseBodyHelper
{
    public static CommonResponseBodyViewModel ConvertToViewModel(this CommonResponseBody commonResponseBody)
    {
        CommonResponseBodyViewModel CommonResponseBodyViewModel = new()
        {
            Id = commonResponseBody.Id,
            Body = commonResponseBody.Body,
            CreatedBy = commonResponseBody.CreatedBy,
            CreatedDate = commonResponseBody.CreatedDate,
            Date = commonResponseBody.Date,
            DraftBody = commonResponseBody.DraftBody,
            DraftTitle = commonResponseBody.DraftTitle,
            ModifiedBy = commonResponseBody.ModifiedBy,
            ModifiedDate = commonResponseBody.ModifiedDate,
            Published = commonResponseBody.Published,
            Title = commonResponseBody.Title,
            Archived = commonResponseBody.Archived,
            Pinned = commonResponseBody.Pinned
        };
        return CommonResponseBodyViewModel;
    }
}
