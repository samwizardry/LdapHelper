using System.DirectoryServices.Protocols;
using System.Text;

using LdapHelper.Exceptions;

namespace LdapHelper;

public class ActiveDirectoryService
{
    private readonly LdapConnection _connection;

    public ActiveDirectoryService(LdapConnection connection)
    {
        _connection = connection;
    }

    public SearchResponse SendSearchRequest(
        string distinguishedName,
        string filter,
        SearchScope searchScope,
        params string[] attributes)
    {
#if LDAPHELPER_DEBUG
        Console.WriteLine($"""
            LDAPv3
            Base: "{distinguishedName}" with scope {searchScope}
            Filter: {filter}
            Requesting: {string.Join(' ', attributes)}
            """);
#endif

        return (SearchResponse)_connection.SendRequest(new SearchRequest(distinguishedName, filter, searchScope, attributes));
    }

    public ModifyResponse SendModifyRequest(
        string distinguishedName,
        params DirectoryAttributeModification[] modifications)
    {
#if LDAPHELPER_DEBUG
        Console.WriteLine($"""
            dn: {distinguishedName}
            changetype: modify
            """);

        foreach (var mod in modifications)
        {
            Console.WriteLine($"""
                {mod.Operation}: {mod.Name}
                """);
        }
#endif

        return (ModifyResponse)_connection.SendRequest(new ModifyRequest(distinguishedName, modifications));
    }

    public IEnumerable<TUser> GetUsers<TUser>(string[] attributes, string queryBase, string filter)
        where TUser : class, IUser<TUser>, new()
    {
        SearchResponse response = SendSearchRequest(
            queryBase,
            filter,
            SearchScope.Subtree,
            attributes);

        if (response.ResultCode != ResultCode.Success)
        {
            throw new SearchRequestException(response.ResultCode, response.ErrorMessage, response);
        }

        foreach (SearchResultEntry entry in response.Entries)
        {
            if (TUser.ParseUser(entry) is TUser user)
            {
                yield return user;
            }
        }
    }

    public void ChangePassword(string distinguishedName, string oldPassword, string newPassword)
    {
        var removePwdMod = new DirectoryAttributeModification();
        removePwdMod.Operation = DirectoryAttributeOperation.Delete;
        removePwdMod.Name = "unicodePwd";
        removePwdMod.Add(Encoding.Unicode.GetBytes($"\"{oldPassword}\""));

        var addPwdMod = new DirectoryAttributeModification();
        addPwdMod.Operation = DirectoryAttributeOperation.Add;
        addPwdMod.Name = "unicodePwd";
        addPwdMod.Add(Encoding.Unicode.GetBytes($"\"{newPassword}\""));

        var response = SendModifyRequest(distinguishedName, removePwdMod, addPwdMod);

        if (response.ResultCode != ResultCode.Success)
        {
            throw new ModifyRequestException(response.ResultCode, response.ErrorMessage, response);
        }
    }

    public void Authenticate(ILdapConnectionFactory connectionFactory, string username, string password)
    {
        var connection = connectionFactory.CreateConnection(username, password);
        connection.Bind();
    }

    public bool TryAuthenticate(ILdapConnectionFactory connectionFactory, string username, string password)
    {
        try
        {
            Authenticate(connectionFactory, username, password);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
