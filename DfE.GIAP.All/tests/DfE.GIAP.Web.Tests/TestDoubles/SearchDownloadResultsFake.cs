using DfE.GIAP.Domain.Models.User;

namespace DfE.GIAP.Web.Tests.TestDoubles
{
    public class SearchDownloadResultsFake
    {
        public Organisation GetOrganisation()
        {
            return new Organisation() { Id = "00000000-0000-0000-0000-000000000000",
                                        Name = "DSI TEST Establishment",
                                        UniqueIdentifier = "Test_UID",
                                        UniqueReferenceNumber = "Test_URN",
                                        StatutoryLowAge = "2",
                                        StatutoryHighAge = "25" };
        }
    }
}
