using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace DfE.GIAP.Web.Helpers.Controllers
{
    /// <summary>
    /// Methods used to extend the standard behaviour
    /// of <see cref="ITempDataDictionary"/> types.
    /// </summary>
    public static class TempDataDictionaryExtensions
    {
        /// <summary>
        /// Acquires the <see cref="TViewModel"/> instance from
        /// the provisioned <see cref="ITempDataDictionary"/>
        /// identified by the specified key.
        /// </summary>
        /// <typeparam name="TViewModel">
        /// The object type to persist between controller requests.
        /// </typeparam>
        /// <param name="tempData">
        /// The controller specific temp data store in which to store the object.
        /// </param>
        /// <param name="persistenceKey">
        /// The key used to access the required temp data store.
        /// </param>
        /// <param name="keepTempDataBetweenRequests">
        /// Predicates whether the temp data should be peristed between controller requests.
        /// </param>
        /// <returns></returns>
        public static TViewModel GetPersistedObject<TViewModel>(
            this ITempDataDictionary tempData,
            string persistenceKey,
            bool keepTempDataBetweenRequests = false) where TViewModel : class
        {
            TViewModel viewModel = default;

            if (HasTempDataValue(tempData, persistenceKey)){
                viewModel = tempData[persistenceKey] as TViewModel;
                SetRequestPersistence(tempData, keepTempDataBetweenRequests);
            }
            return viewModel;
        }

        /// <summary>
        /// Add the <see cref="TViewModel"/> instance to the
        /// provisioned <see cref="ITempDataDictionary"/>
        /// identified by the specified key, thus allowing the
        /// object to be persisted between controller requests.
        /// </summary>
        /// <typeparam name="TViewModel">
        /// The object type to persist between controller requests.
        /// </typeparam>
        /// <param name="tempData">
        /// The controller specific temp data store in which to store the object.
        /// </param>
        /// <param name="persistenceObject">
        /// The instance of the TViewModel to persist between controller requests.
        /// </param>
        /// <param name="persistenceKey">
        /// The key used to access the required temp data store.
        /// </param>
        public static void SetPersistedObject<TViewModel>(
            this ITempDataDictionary tempData,
            TViewModel persistenceObject,
            string persistenceKey) where TViewModel : class =>
                tempData[persistenceKey] = persistenceObject;

        private static bool HasTempDataValue(
            this ITempDataDictionary tempData,
            string persistenceKey) => tempData.Peek(persistenceKey) != null;

        private static void SetRequestPersistence(
            this ITempDataDictionary tempData,
            bool keepTempDataBetweenRequests)
        {
            if (keepTempDataBetweenRequests) tempData.Keep();
        }
    }
}
