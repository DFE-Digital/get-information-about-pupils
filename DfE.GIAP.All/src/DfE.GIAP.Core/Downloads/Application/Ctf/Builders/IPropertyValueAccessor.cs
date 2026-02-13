namespace DfE.GIAP.Core.Downloads.Application.Ctf.Builders;

public interface IPropertyValueAccessor
{
    string? GetValue(object instance, string fieldName);
}
