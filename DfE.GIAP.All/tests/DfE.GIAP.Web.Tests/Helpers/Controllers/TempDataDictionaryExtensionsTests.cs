using System;
using DfE.GIAP.Web.Features.Search.LegacyModels;
using DfE.GIAP.Web.Helpers.Controllers;
using DfE.GIAP.Web.Tests.TestDoubles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NSubstitute;
using Xunit;

namespace DfE.GIAP.Web.Tests.Helpers.Controllers;

public class TempDataDictionaryExtensionsTests
{
    [Theory]
    [InlineData("TestTempDataKey", "TestTempDataObject")]
    public void GetPersistedObject_with_object_available_and_valid_key_returns_object_from_temp_data(
        string persistenceKey, object persistedObj)
    {
        // arrange.
        ITempDataDictionary tempData =
            TempDataTestDoubles.StubFor(persistenceKey, persistedObj);

        // act.
        string result = tempData.GetPersistedObject<string>(persistenceKey);

        // assert.
        Assert.NotNull(result);
        Assert.Equal(persistedObj, result);
    }

    [Theory]
    [InlineData("TestTempDataKey", "TestTempDataKey1", "TestTempDataObject")]
    public void GetPersistedObject_with_object_available_and_valid_invalidkey_returns_null_from_temp_data(
        string persistenceKey, string invalidPersistenceKey, object persistedObj)
    {
        // arrange.
        ITempDataDictionary tempData =
            TempDataTestDoubles.StubFor(persistenceKey, persistedObj);

        // act.
        string result = tempData.GetPersistedObject<string>(invalidPersistenceKey);

        // assert.
        Assert.Null(result);
    }

    [Theory]
    [InlineData("TestTempDataKey", "TestTempDataKey")]
    public void SetPersistedObject_with_valid_string_and_key_persists_object_in_temp_data(
        string persistenceKey, object persistedObj)
    {
        // arrange.
        ITempDataDictionary tempData = TempDataTestDoubles.Stub();

        // act.
        tempData.SetPersistedObject(persistedObj, persistenceKey);

        // assert.
        object? result = tempData[persistenceKey];
        Assert.IsType<string>(result);
        Assert.Equal(persistedObj, result);
    }

    [Theory]
    [ClassData(typeof(DobSearchFilterTestData))]
    public void SetPersistedObject_with_valid_user_defined_object_and_key_persists_object_in_temp_data(
        SearchFilters persistedObj)
    {
        // arrange.
        const string TempDataPersistenceKey = "TestTempDataKey";

        ITempDataDictionary tempData = TempDataTestDoubles.Stub();

        // act.
        tempData.SetPersistedObject(persistedObj, TempDataPersistenceKey);

        // assert.
        object? result = tempData[TempDataPersistenceKey];
        Assert.IsType<SearchFilters>(result);
        Assert.Equal(persistedObj, result);
    }

    [Theory]
    [InlineData("TestTempDataKey")]
    public void SetPersistedObject_with_valid_string_and_null_key_throws_exception(
        object persistedObj)
    {
        // arrange.
        ITempDataDictionary tempData = TempDataTestDoubles.Stub();

        // assert.
        Assert.Throws<ArgumentNullException>(() =>
            tempData.SetPersistedObject(persistedObj, null));
    }

    internal static class TempDataTestDoubles
    {
        public static ITempDataDictionary Stub() =>
            new TempDataDictionary(
                new DefaultHttpContext(), Substitute.For<ITempDataProvider>());

        public static ITempDataDictionary StubFor<TObject>(
            string tempDataKey,
            TObject persistedObject) where TObject : class
        {
            ITempDataDictionary mockTempData = Stub();
            mockTempData[tempDataKey] = persistedObject;
            return mockTempData;
        }
    }
}
