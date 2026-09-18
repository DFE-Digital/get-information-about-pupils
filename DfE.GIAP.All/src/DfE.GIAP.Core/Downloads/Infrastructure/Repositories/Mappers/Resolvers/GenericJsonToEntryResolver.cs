using System.Collections;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace DfE.GIAP.Core.Downloads.Infrastructure.Repositories.Mappers.Resolvers;

public static class GenericJsonToEntryResolver
{
    public static TEntry Resolve<TSource, TEntry>(
        TSource? source,
        List<FieldMappingDefinition> schema)
        where TEntry : new()
    {
        TEntry entry = new();

        if (source == null)
            return entry;

        // Extract raw fields from either RawCosmosDocument or JObject
        Dictionary<string, JToken>? rawFields = ExtractRawFields(source);

        if (rawFields is not null)
        {
            ResolveSimpleFields(entry, rawFields, schema);
            ResolveNestedCollections(entry, rawFields);
        }

        return entry;
    }

    private static Dictionary<string, JToken>? ExtractRawFields<TSource>(TSource source)
    {
        // Case 1: RawCosmosDocument with Fields or RawFields property
        PropertyInfo[] sourceProps = typeof(TSource).GetProperties();
        PropertyInfo? rawFieldsProp = sourceProps.FirstOrDefault(p => p.Name is "Fields" or "RawFields");

        if (rawFieldsProp is not null)
        {
            if (rawFieldsProp.GetValue(source) is Dictionary<string, JToken> fields)
                return fields;
        }

        // Case 2: Nested JObject
        if (source is JObject jObj)
        {
            return jObj.Properties()
                       .ToDictionary(p => p.Name, p => p.Value);
        }

        return null;
    }

    private static void ResolveSimpleFields<TEntry>(
        TEntry entry,
        Dictionary<string, JToken> rawFields,
        List<FieldMappingDefinition> schema)
    {
        PropertyInfo[] entryProps = typeof(TEntry).GetProperties();

        foreach (FieldMappingDefinition field in schema)
        {
            // Find the first matching JSON field name
            string? match = field.SourceNames
                .FirstOrDefault(src => rawFields.ContainsKey(src));

            if (match is null)
                continue;

            JToken token = rawFields[match];

            // Find the target property on the entity
            PropertyInfo? targetProp = entryProps.FirstOrDefault(p =>
                p.Name.Equals(field.TargetProperty, StringComparison.OrdinalIgnoreCase));

            if (targetProp == null)
                continue;

            object? converted = ConvertToType(token, field.TargetType);
            targetProp.SetValue(entry, converted);
        }
    }

    private static void ResolveNestedCollections<TEntry>(
        TEntry entry,
        Dictionary<string, JToken> rawFields)
    {
        PropertyInfo[] entryProps = typeof(TEntry).GetProperties();

        foreach (PropertyInfo prop in entryProps)
        {
            // Only handle List<T> properties
            if (!prop.PropertyType.IsGenericType ||
                prop.PropertyType.GetGenericTypeDefinition() != typeof(List<>))
                continue;

            Type elementType = prop.PropertyType.GetGenericArguments()[0];

            // Look for a matching field name (case-insensitive)
            KeyValuePair<string, JToken> match = rawFields
                .FirstOrDefault(kvp => kvp.Key.Equals(prop.Name, StringComparison.OrdinalIgnoreCase));

            if (match.Key is null)
                continue;

            if (match.Value is not JArray array)
                continue;

            List<FieldMappingDefinition>? nestedSchema = SchemaRegistry.GetSchemaFor(elementType);
            if (nestedSchema is null)
                continue;

            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType))!;

            foreach (JObject item in array.OfType<JObject>())
            {
                object? resolved = typeof(GenericJsonToEntryResolver)
                    .GetMethod(nameof(Resolve))!
                    .MakeGenericMethod(typeof(JObject), elementType)
                    .Invoke(null, new object[] { item, nestedSchema });

                list.Add(resolved);
            }

            prop.SetValue(entry, list);
        }
    }

    private static object? ConvertToType(JToken token, Type type)
    {
        if (token == null || token.Type == JTokenType.Null)
            return null;

        if (type == typeof(DateTime?) || type == typeof(DateTime))
            return token.Value<DateTime?>();

        if (type == typeof(int?) || type == typeof(int))
            return token.Value<int?>();

        if (type == typeof(decimal?) || type == typeof(decimal))
            return token.Value<decimal?>();

        if (type == typeof(char?) || type == typeof(char))
            return token.Value<char?>();

        return token.ToObject(type);
    }
}

