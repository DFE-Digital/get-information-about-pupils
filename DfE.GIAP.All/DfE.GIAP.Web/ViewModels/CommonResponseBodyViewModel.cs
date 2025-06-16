using DfE.GIAP.Common.Constants.Messages.Common;
using DfE.GIAP.Domain.Models.User;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class CommonResponseBodyViewModel
{
    [Required(ErrorMessage = CommonErrorMessages.AdminBodyRequired)]
    public string Body { get; set; }
    public UserInfo CreatedBy { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime Date { get; set; }
    public string DraftBody { get; set; }
    public string DraftTitle { get; set; }
    public string Id { get; set; }
    public UserInfo ModifiedBy { get; set; }
    public DateTime ModifiedDate { get; set; }
    public bool Published { get; set; }

    [Required(ErrorMessage = CommonErrorMessages.AdminTitleRequired)]
    [MaxLength(64, ErrorMessage = CommonErrorMessages.AdminTitleLength)]
    public string Title { get; set; }
    public bool Archived { get; set; }
    public bool Pinned { get; set; }
}
