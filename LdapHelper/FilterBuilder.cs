using System.Text;

namespace LdapHelper;

public sealed class FilterBuilder
{
    private readonly StringBuilder _filter = new("(&");

    public string Build()
    {
        _filter.Append(")");
        return _filter.ToString();
    }

    public FilterBuilder AddObjectCategory(string objectCategory = "person")
    {
        _filter.Append($"(objectCategory={objectCategory})");
        return this;
    }

    public FilterBuilder AddObjectClass(string objectClass = "user")
    {
        _filter.Append($"(objectClass={objectClass})");
        return this;
    }

    public FilterBuilder AddAccountEnabledFilter()
    {
        _filter.Append($"(!({Constants.AccountDisabledFilter}))");
        return this;
    }

    public FilterBuilder AddAccountDisabledFilter()
    {
        _filter.Append($"({Constants.AccountDisabledFilter})");
        return this;
    }

    public FilterBuilder AddMemberOf(string group)
    {
        _filter.Append($"(memberOf={group})");
        return this;
    }

    /// <summary>
    /// Operator "OR".
    /// </summary>
    public FilterBuilder AddMemberOf(params string[] groups)
    {
        _filter.Append("(|");
        foreach (var group in groups)
        {
            _filter.Append($"(memberOf={group})");
        }
        _filter.Append(")");

        return this;
    }

    public FilterBuilder AddUpn(string upn)
    {
        _filter.Append($"(userPrincipalName={upn})");
        return this;
    }
}
