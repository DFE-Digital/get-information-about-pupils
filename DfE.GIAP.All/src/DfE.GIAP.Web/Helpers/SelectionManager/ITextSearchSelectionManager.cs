namespace DfE.GIAP.Web.Helpers.SelectionManager
{
    public interface ITextSearchSelectionManager
    {
        public void Add(string selectedLearnerNumber);
        public void Clear();

        public string GetSelectedFromSession();
    }
}
