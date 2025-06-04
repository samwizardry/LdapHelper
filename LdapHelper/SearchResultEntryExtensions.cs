using System.DirectoryServices.Protocols;

namespace LdapHelper;

public static class SearchResultEntryExtensions
{
    public static T? GetAttributeValue<T>(this SearchResultEntry entry, string attributeName)
    {
        if (!entry.Attributes.Contains(attributeName))
            return default;

        var attribute = entry.Attributes[attributeName];

        if (attribute.Count == 0)
            return default;

        if (attribute[0] is not T value)
            return default;

        return value;
    }
}
