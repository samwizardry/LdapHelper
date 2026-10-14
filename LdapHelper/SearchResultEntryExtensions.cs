using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Text;

namespace LdapHelper;

public static class SearchResultEntryExtensions
{
    private static readonly string[] GeneralizedTimeFormats =
    [
        "yyyyMMddHHmmss.fZ",
        "yyyyMMddHHmmss.ffZ",
        "yyyyMMddHHmmss.fffZ",
        "yyyyMMddHHmmss.ffffZ",
        "yyyyMMddHHmmss.fffffZ",
        "yyyyMMddHHmmss.ffffffZ",
        "yyyyMMddHHmmss.0Z",
        "yyyyMMddHHmmssZ"
    ];

    public static T? GetAttributeValue<T>(this SearchResultEntry entry, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeName);

        if (!entry.Attributes.Contains(attributeName))
            return default;

        DirectoryAttribute attribute = entry.Attributes[attributeName];
        if (attribute.Count == 0)
            return default;

        object firstValue = attribute[0];

        if (firstValue is T typedValue)
            return typedValue;

        if (typeof(T) == typeof(string))
        {
            if (firstValue is byte[] bytes)
                return (T)(object)Encoding.UTF8.GetString(bytes);

            return (T)(object)firstValue.ToString()!;
        }

        if (typeof(T) == typeof(byte[]))
        {
            if (firstValue is string str)
                return (T)(object)Encoding.UTF8.GetBytes(str);
        }

        if (typeof(T) == typeof(Guid) || typeof(T) == typeof(Guid?))
        {
            if (firstValue is byte[] guidBytes && guidBytes.Length == 16)
                return (T)(object)new Guid(guidBytes);

            if (firstValue is string guidStr && Guid.TryParse(guidStr, out Guid parsedGuid))
                return (T)(object)parsedGuid;
        }

        if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
        {
            if (int.TryParse(firstValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int intVal))
                return (T)(object)intVal;
        }

        if (typeof(T) == typeof(long) || typeof(T) == typeof(long?))
        {
            if (long.TryParse(firstValue.ToString(), NumberStyles.Integer, CultureInfo.InvariantCulture, out long longVal))
                return (T)(object)longVal;
        }

        return default;
    }

    public static string? GetString(this SearchResultEntry entry, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeName);

        if (!entry.Attributes.Contains(attributeName))
            return null;

        DirectoryAttribute attribute = entry.Attributes[attributeName];
        if (attribute.Count == 0)
            return null;

        object value = attribute[0];
        return value switch
        {
            string str => str,
            byte[] bytes => Encoding.UTF8.GetString(bytes),
            _ => value.ToString()
        };
    }

    public static IReadOnlyList<string> GetStrings(this SearchResultEntry entry, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeName);

        if (!entry.Attributes.Contains(attributeName))
            return [];

        DirectoryAttribute attribute = entry.Attributes[attributeName];
        if (attribute.Count == 0)
            return [];

        var list = new List<string>(attribute.Count);
        foreach (object value in attribute)
        {
            string? parsed = value switch
            {
                string str => str,
                byte[] bytes => Encoding.UTF8.GetString(bytes),
                _ => value.ToString()
            };

            if (parsed is not null)
                list.Add(parsed);
        }

        return list;
    }

    public static byte[]? GetBytes(this SearchResultEntry entry, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeName);

        if (!entry.Attributes.Contains(attributeName))
            return null;

        DirectoryAttribute attribute = entry.Attributes[attributeName];
        if (attribute.Count == 0)
            return null;

        object value = attribute[0];
        return value switch
        {
            byte[] bytes => bytes,
            string str => Encoding.UTF8.GetBytes(str),
            _ => null
        };
    }

    public static Guid? GetGuid(this SearchResultEntry entry, string attributeName)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentException.ThrowIfNullOrWhiteSpace(attributeName);

        if (!entry.Attributes.Contains(attributeName))
            return null;

        DirectoryAttribute attribute = entry.Attributes[attributeName];
        if (attribute.Count == 0)
            return null;

        object value = attribute[0];
        if (value is byte[] bytes && bytes.Length == 16)
            return new Guid(bytes);

        if (value is string str && Guid.TryParse(str, out Guid guid))
            return guid;

        return null;
    }

    public static int? GetInt32(this SearchResultEntry entry, string attributeName)
    {
        string? str = entry.GetString(attributeName);
        if (str is not null && int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int val))
            return val;

        return null;
    }

    public static long? GetInt64(this SearchResultEntry entry, string attributeName)
    {
        string? str = entry.GetString(attributeName);
        if (str is not null && long.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out long val))
            return val;

        return null;
    }

    public static DateTimeOffset? GetGeneralizedTime(this SearchResultEntry entry, string attributeName)
    {
        string? str = entry.GetString(attributeName);
        if (string.IsNullOrWhiteSpace(str))
            return null;

        if (DateTimeOffset.TryParseExact(str, GeneralizedTimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset dto))
            return dto;

        if (DateTimeOffset.TryParse(str, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset fallback))
            return fallback;

        return null;
    }

    public static DateTimeOffset? GetFileTime(this SearchResultEntry entry, string attributeName)
    {
        long? raw = entry.GetInt64(attributeName);
        if (raw is null || raw <= 0)
            return null;

        try
        {
            return DateTimeOffset.FromFileTime(raw.Value).ToUniversalTime();
        }
        catch (ArgumentOutOfRangeException)
        {
            return null;
        }
    }
}
