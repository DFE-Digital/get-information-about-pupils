using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Web.Features.Auth.Application.Models;
using DfE.GIAP.Web.Features.Auth.Infrastructure.DataTransferObjects;

namespace DfE.GIAP.Web.Features.Auth.Infrastructure.Mappers;

internal class OrganisationDtoToEntityOrganisation : IMapper<OrganisationDto, Organisation>
{
    public Organisation Map(OrganisationDto input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input), "Mapping input cannot be null.");
        }

        return new Organisation
        {
            Id = input.Id,
            Name = input.Name,
            Category = input.Category is not null
            ? new OrganisationCategory { Id = input.Category.Id }
            : null,
            EstablishmentType = input.EstablishmentType is not null
            ? new EstablishmentType { Id = input.EstablishmentType.Id }
            : null,
            StatutoryLowAge = input.StatutoryLowAge,
            StatutoryHighAge = input.StatutoryHighAge,
            EstablishmentNumber = input.EstablishmentNumber,
            LocalAuthority = input.LocalAuthority is not null
            ? new LocalAuthority { Code = input.LocalAuthority.Code }
            : null,
            UniqueReferenceNumber = input.UniqueReferenceNumber,
            UniqueIdentifier = input.UniqueIdentifier,
            UKProviderReferenceNumber = input.UKProviderReferenceNumber
        };
    }
}
