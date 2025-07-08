using System;
using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Core.Models.Glossary
{
    [ExcludeFromCodeCoverage]
    public class MetaDataDownload
    {
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }
    }
}
