using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Domain.Models.Common;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.BlobStorage
{
    public interface IBlobStorageService
    {
        Task<List<MetaDataDownload>> GetFileList(string directory);

        Task DownloadFileAsync(string directory, string fileName, Stream stream, AzureFunctionHeaderDetails azureFunctionHeaderDetails);
    }
}
