namespace DfE.GIAP.Core.MyPupils.Infrastructure.Repositories.DataTransferObjects;

#nullable disable
public sealed class MyPupilsDocumentDto
{
    public string id { get; set; }
    public MyPupilsDto MyPupils { get; set; }
}
