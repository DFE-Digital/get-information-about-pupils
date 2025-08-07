using DfE.GIAP.Core.Common.CrossCutting;
using Dto = DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.DataTransferObjects;
using Model = DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;

namespace DfE.GIAP.Core.Search.FurtherEducation.Infrastructure.Mappers;

/// <summary>
/// 
/// </summary>
public sealed class SearchResultToFurtherEducationPupilMapper : IMapper<Dto.FurtherEducationPupil, Model.FurtherEducationPupil>
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public Model.FurtherEducationPupil Map(Dto.FurtherEducationPupil input)
    {
        //ArgumentException.ThrowIfNullOrEmpty(input.Id);
        ArgumentException.ThrowIfNullOrEmpty(input.ULN);
        ArgumentException.ThrowIfNullOrEmpty(input.Forename);
        ArgumentException.ThrowIfNullOrEmpty(input.Surname);

        return new(
            uniqueLearnerNumber: input.ULN,
            surname: input.Surname,
            forename: input.Forename);
    }
}
