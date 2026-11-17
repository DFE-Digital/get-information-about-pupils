namespace DfE.GIAP.Web.ViewModels.Search;

public record LearnerNumberErrorTableViewModel(string Heading, IEnumerable<string> Numbers, string Id, bool ShowErrors);
