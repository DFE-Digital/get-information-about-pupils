using DfE.GIAP.Core.Common.CrossCutting;
using DfE.GIAP.Core.Search.Common.Application.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Models;
using DfE.GIAP.Core.Search.FurtherEducation.Application.UseCases.SearchByFirstnameAndOrSurname.Response;
using DfE.GIAP.Domain.Search.Learner;
using DfE.GIAP.Web.ViewModels.Search;

namespace DfE.GIAP.Web.Controllers.TextBasedSearch.Mappers;

/// <summary>
/// TODO: we'll sort this probs best not to use a tuple here, just using at the mo for convenience!
/// </summary>
public sealed class LearnerSearchResponseToViewModelMapper :
    IMapper<(LearnerTextSearchViewModel, SearchByFirstNameAndOrSurnameResponse), LearnerTextSearchViewModel>
{
    private readonly IMapper<FurtherEducationLearner, Learner> _furtherEducationLearnerToViewModelMapper;
    private readonly IMapper<SearchFacets, List<FilterData>> _filtersResponseMapper;

    public LearnerSearchResponseToViewModelMapper(
        IMapper<FurtherEducationLearner, Learner> furtherEducationLearnerToViewModelMapper,
        IMapper<SearchFacets, List<FilterData>> filtersResponseMapper)
    {
        _furtherEducationLearnerToViewModelMapper = furtherEducationLearnerToViewModelMapper ??
            throw new ArgumentNullException(nameof(furtherEducationLearnerToViewModelMapper));
        _filtersResponseMapper = filtersResponseMapper ??
            throw new ArgumentNullException(nameof(filtersResponseMapper));
    }

    public LearnerTextSearchViewModel Map((LearnerTextSearchViewModel, SearchByFirstNameAndOrSurnameResponse) input)
    {
        LearnerTextSearchViewModel model = input.Item1;
        SearchByFirstNameAndOrSurnameResponse result = input.Item2;

        // Map facet results to filters.
        model.Filters = _filtersResponseMapper.Map(result.FacetedResults);


        //ParseGender(ref result);
        //ParseSex(ref result);

        //var lowAge = User.GetOrganisationLowAge();
        //var highAge = User.GetOrganisationHighAge();

        IList<Learner> learners =
            result.LearnerSearchResults.Learners
                .Select(learner => _furtherEducationLearnerToViewModelMapper.Map(learner))
                .ToList();

        if (result.TotalNumberOfResults > model.MaximumResults)
        {
            model.Learners = learners.Take(model.MaximumResults).ToList();
        }
        else
        {
            model.Learners = learners; // need a mapping here!!!!
        }

        model.Count = result.TotalNumberOfResults;
        model.Total = result.LearnerSearchResults.Count;

        //model.Filters = result.Filters; // need to map back from facets

        //SetLearnerNumberId(model);

        //var isAdmin = User.IsAdmin();
        //if (!isAdmin && indexType != AzureSearchIndexType.FurtherEducation)
        //{
        //    model.Learners = RbacHelper.CheckRbacRulesGeneric<Learner>(model.Learners.ToList(), lowAge, highAge);
        //}

        //var selected = GetSelected();

        //if (!string.IsNullOrEmpty(selected))
        //{
        //    foreach (var learner in model.Learners)
        //    {
        //        if (!string.IsNullOrEmpty(learner.LearnerNumberId))
        //        {
        //            learner.Selected = selected.Contains(learner.LearnerNumberId);
        //        }
        //    }
        //}
        model.Learners = learners;

        //model.PageLearnerNumbers = String.Join(',', model.Learners.Select(l => l.LearnerNumberId));

        model.ShowOverLimitMessage = model.Total > 100000;

        return model;



    }



    //private void SetLearnerNumberId(LearnerTextSearchViewModel model)
    //{

    //    foreach (var learner in model.Learners)
    //    {
    //        learner.LearnerNumberId = learner.LearnerNumber switch
    //        {
    //            "0" => learner.Id,
    //            _ => learner.LearnerNumber,
    //        };

    //    }

    //}


    //private void ParseGender(ref SearchByFirstNameAndOrSurnameResponse result)
    //{
    //    var genderFilter = result.FacetedResults.Filters.Where(filterData =>
    //        filterData.Name.Equals("Gender")).ToList();

    //    genderFilter.ForEach(filterData =>
    //        filterData.Items.ForEach(filterDataItem =>
    //            filterDataItem.Value = filterDataItem.Value.SwitchGenderToParseName()));
    //}

    //private void ParseSex(ref SearchByFirstNameAndOrSurnameResponse result)
    //{
    //    var sexFilter = result.Filters.Where(filterData =>
    //        filterData.Name.Equals("Sex")).ToList();

    //    sexFilter.ForEach(filterData =>
    //        filterData.Items.ForEach(filterDataItem =>
    //            filterDataItem.Value = filterDataItem.Value.SwitchSexToParseName()));
    //}
}
