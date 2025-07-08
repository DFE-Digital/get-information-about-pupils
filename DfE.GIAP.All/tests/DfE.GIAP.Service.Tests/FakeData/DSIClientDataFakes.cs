using DfE.GIAP.Domain.Models.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DfE.GIAP.Service.Tests.FakeData
{
    public class DSIClientDataFakes
    {
        public static UserAccess GetValidSingleData()
        {

            var roles = new List<UserRole>();
            roles.Add(new UserRole
            {
                Id = Guid.Parse("0E49A3D1-A7E9-4241-ADE2-08442519E74C"),
                Code = "GIAPAdmin",
                Name = "GIAP Admin"
            });


            var identifiers = new List<Domain.Models.Common.KeyValue>();
            identifiers.Add(new Domain.Models.Common.KeyValue
            {
                Key = "groups",
                Value = "GIAPAdmin"
            });

            var userAccess = new UserAccess
            {
                UserId = Guid.Parse("97394E3E-F5C6-402E-B230-103C67F30280"),
                OrganisationId = Guid.Parse("23F20E54-79EA-4146-8E39-18197576F023"),
                ServiceId = Guid.Parse("C18830DB-56D2-48EF-8B1D-917FC05BAD82"),
                Roles = roles,
                Identifiers = identifiers
            };

            return userAccess;
        }

        public static string GetValidSingleDataAsString()
        {
            return JsonConvert.SerializeObject(GetValidSingleData());
        }

        public static List<Organisation> GetOrganisations()
        {
            var organisations = new List<Organisation>();
            organisations.Add(new Organisation()
            {
                Id = "23F20E54-79EA-4146-8E39-18197576F023",
                Name = "Department for Education",
                EstablishmentNumber = "001",

            });
            return organisations;
        }

        public static string GetOrganisationsAsString()
        {
            return JsonConvert.SerializeObject(GetOrganisations());
        }

    }
}
