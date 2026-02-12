using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Web.Features.Search.LegacyModels;

[ExcludeFromCodeCoverage]
public class SearchDownloadDataType
{
    public string Name { get; set; }

    public string Value { get; set; }

    public bool CanDownload { get; set; }

    public bool Disabled { get; set; }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Value, CanDownload);
    }

    public override bool Equals(object obj)
    {
        if (obj is SearchDownloadDataType type)
        {
            return type.Name.Equals(Name) && type.Value.Equals(Value) && type.CanDownload == CanDownload;
        }

        return false;
    }
}
