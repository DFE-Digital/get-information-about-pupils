namespace DfE.GIAP.Web.ViewModels;

public class PreparedFileViewModel
{
    public string Name { get; set; }

    public DateTime Date { get; set; }

    public string FileName =>
         string.IsNullOrEmpty(Name) ? null : Name.Split('/').Last();

    public string DisplayName =>
        string.IsNullOrEmpty(FileName) ? null :
        FileName.Contains('.') ? FileName.Substring(0, FileName.LastIndexOf('.')) : FileName;

    public string DisplayDate => Date.ToString("dd/MM/yyyy");
}
