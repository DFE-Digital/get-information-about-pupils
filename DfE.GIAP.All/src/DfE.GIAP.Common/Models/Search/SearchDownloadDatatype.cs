using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Search
{
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
                return type.Name.Equals(this.Name) && type.Value.Equals(this.Value) && type.CanDownload == this.CanDownload;
            }

            return false;
        }
    }
}
