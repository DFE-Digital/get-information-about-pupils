using System.Diagnostics.CodeAnalysis;

namespace DfE.GIAP.Domain.Models.Common
{
    [ExcludeFromCodeCoverage]
    public class ReturnFile
    {
        public byte[] Bytes { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
        public string[] RemovedUpns { get; set; }
        public string ResponseMessage { get; set; }
    }
}
