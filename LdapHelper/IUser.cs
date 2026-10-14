using System.DirectoryServices.Protocols;

namespace LdapHelper;

public interface IUser
{
    Guid? ObjectGuid { get; }

    string? SamAccountName { get; }

    string? UserPrincipalName { get; }

    string? DistinguishedName { get; }

    string? Email { get; }
}

public interface IUser<TSelf> : IUser
    where TSelf : class, IUser<TSelf>
{
    static abstract TSelf? ParseUser(SearchResultEntry entry);
}
