namespace DfE.GIAP.Core.Downloads.Application.UseCases.DownloadPupilCtf.Ctf.Builders;

public interface IPropertyValueAccessor
{
    string? GetValue(object instance, string fieldName);
}
