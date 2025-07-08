using Azure.Storage.Blobs;
using DfE.GIAP.Common.AppSettings;
using DfE.GIAP.Core.Models.Glossary;
using DfE.GIAP.Domain.Models.Common;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DfE.GIAP.Service.ApplicationInsightsTelemetry;

namespace DfE.GIAP.Service.BlobStorage
{
    [ExcludeFromCodeCoverage]
    public class BlobStorageService: IBlobStorageService
    {
        private readonly BlobContainerClient _container;
        private readonly AzureAppSettings _azureAppSettings;
        private readonly IEventLogging _eventLogging;
        private readonly IHostEnvironment _hostEnvironment;

        public BlobStorageService(IOptions<AzureAppSettings> azureAppSettings, IEventLogging eventLogging, IHostEnvironment hostEnvironment)
        {
            _azureAppSettings = azureAppSettings.Value;
            _container = new BlobServiceClient($"DefaultEndpointsProtocol=https;AccountName={_azureAppSettings.StorageAccountName};AccountKey={_azureAppSettings.StorageAccountKey}")  //;EndpointSuffix=core.windows.net
                .GetBlobContainerClient(_azureAppSettings.StorageContainerName);
            _eventLogging = eventLogging;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<List<MetaDataDownload>> GetFileList(string directory)
        {
            try
            {
                return _container
                     .GetBlobs(prefix:directory)?
                     .Select(b => new MetaDataDownload()
                     {
                         Name = b.Name,
                         FileName = b.Name,
                         Date = b.Properties.LastModified.Value.Date
                     }).ToList();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task DownloadFileAsync(string directory, string fileName, Stream stream, AzureFunctionHeaderDetails azureFunctionHeaderDetails)
        {
            try
            {
                _eventLogging.TrackEvent(1121, $"Pre-prepared download initiated", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);

                var blockBlob = _container.GetBlobClient($"{directory}{fileName}");


                if (!await blockBlob.ExistsAsync())
                {
                    _eventLogging.TrackEvent(2500, $"Pre-prepared download not found", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                    return;
                }

                await blockBlob.DownloadToAsync(stream);
                _eventLogging.TrackEvent(1122, $"Pre-prepared download successful", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
            }
            catch (Exception ex)
            {
                _eventLogging.TrackEvent(2501, $"An unhandled exception occurred", azureFunctionHeaderDetails.ClientId, azureFunctionHeaderDetails.SessionId, _hostEnvironment.ContentRootPath);
                return;
            }
        }
    }
}
