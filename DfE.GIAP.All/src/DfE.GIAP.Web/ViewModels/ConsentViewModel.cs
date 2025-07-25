using DfE.GIAP.Core.Contents.Application.Models;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.ViewModels;

[ExcludeFromCodeCoverage]
public class ConsentViewModel
{
    public bool ConsentGiven { get; set; }
    public bool HasError { get; set; }
}
