using DfE.GIAP.Common.AppSettings;

namespace DfE.GIAP.Service.Tests.FakeData
{
    public class DownloadServiceResultsFake
    {
        public List<MetaDataDownload> GetMetaDataDetailsList()
        {
            var list = new List<MetaDataDownload>();

            list.Add(new MetaDataDownload() { Name = "Test Name", FileName = "Test File Name", Date = DateTime.Now, Link = "Test Link" });
            list.Add(new MetaDataDownload() { Name = "Test Name1", FileName = "Test File Name1", Date = DateTime.Now, Link = "Test Link1" });

            return list;
        }

        public AzureAppSettings GetAppSettings()
        {
            var appSettings = new AzureAppSettings() { DownloadPupilsByUPNsCSVUrl= "https://www.somewhere.com" };

            return appSettings;
        }
    }
}
