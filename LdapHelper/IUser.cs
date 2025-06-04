using System.DirectoryServices.Protocols;

namespace LdapHelper;

public interface IUser<TUser>
    where TUser : class, new()
{
    static abstract TUser? ParseUser(SearchResultEntry entry);
}
