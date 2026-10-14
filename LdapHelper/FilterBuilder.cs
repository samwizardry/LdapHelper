using System.Text;

namespace LdapHelper;

public sealed class FilterBuilder
{
    private readonly List<string> _conditions = [];

    public static string EscapeFilterValue(string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length);
        foreach (char c in value)
        {
            switch (c)
            {
                case '*':
                    sb.Append(@"\2a");
                    break;
                case '(':
                    sb.Append(@"\28");
                    break;
                case ')':
                    sb.Append(@"\29");
                    break;
                case '\\':
                    sb.Append(@"\5c");
                    break;
                case '\0':
                    sb.Append(@"\00");
                    break;
                default:
                    sb.Append(c);
                    break;
            }
        }

        return sb.ToString();
    }

    public string Build()
    {
        if (_conditions.Count == 0)
            return "(&(objectClass=*))";

        var sb = new StringBuilder("(&");
        foreach (string condition in _conditions)
        {
            sb.Append(condition);
        }
        sb.Append(')');

        return sb.ToString();
    }

    public FilterBuilder AddRaw(string rawCondition)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(rawCondition);

        string trimmed = rawCondition.Trim();
        if (!trimmed.StartsWith('(') || !trimmed.EndsWith(')'))
            trimmed = $"({trimmed})";

        _conditions.Add(trimmed);
        return this;
    }

    public FilterBuilder AddEquals(string attribute, string value, bool escape = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(attribute);
        ArgumentNullException.ThrowIfNull(value);

        string safeValue = escape ? EscapeFilterValue(value) : value;
        _conditions.Add($"({attribute}={safeValue})");
        return this;
    }

    public FilterBuilder AddObjectCategory(string objectCategory = "person") =>
        AddEquals("objectCategory", objectCategory);

    public FilterBuilder AddObjectClass(string objectClass = "user") =>
        AddEquals("objectClass", objectClass);

    public FilterBuilder AddAccountEnabledFilter()
    {
        _conditions.Add($"(!({Constants.AccountDisabledFilter}))");
        return this;
    }

    public FilterBuilder AddAccountDisabledFilter()
    {
        _conditions.Add($"({Constants.AccountDisabledFilter})");
        return this;
    }

    public FilterBuilder AddMemberOf(string group)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(group);
        // In DNs, parentheses must still be escaped for LDAP filters
        string safeGroup = group.Replace("(", @"\28").Replace(")", @"\29");
        _conditions.Add($"(memberOf={safeGroup})");
        return this;
    }

    /// <summary>
    /// Matches users who are a member of ANY of the specified groups (OR logic).
    /// </summary>
    public FilterBuilder AddMemberOf(params string[] groups)
    {
        if (groups is null || groups.Length == 0)
            return this;

        if (groups.Length == 1)
            return AddMemberOf(groups[0]);

        var sb = new StringBuilder("(|");
        foreach (string group in groups)
        {
            if (string.IsNullOrWhiteSpace(group))
                continue;

            string safeGroup = group.Replace("(", @"\28").Replace(")", @"\29");
            sb.Append($"(memberOf={safeGroup})");
        }
        sb.Append(')');

        _conditions.Add(sb.ToString());
        return this;
    }

    public FilterBuilder AddUpn(string upn) =>
        AddEquals("userPrincipalName", upn);

    public FilterBuilder AddSamAccountName(string samAccountName) =>
        AddEquals("sAMAccountName", samAccountName);

    public FilterBuilder AddEmail(string email) =>
        AddEquals("mail", email);

    public FilterBuilder Clear()
    {
        _conditions.Clear();
        return this;
    }
}
