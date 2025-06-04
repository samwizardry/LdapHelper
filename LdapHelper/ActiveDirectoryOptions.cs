namespace LdapHelper;

public class ActiveDirectoryOptions
{
    /// <summary>
    /// e.g. "DC=contoso,DC=com"
    /// </summary>
    public string QueryBase { get; init; } = null!;
}
