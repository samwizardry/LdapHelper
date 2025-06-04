using System.DirectoryServices.Protocols;
using System.Text;

using LdapHelper.Exceptions;

namespace LdapHelper;

public class ActiveDirectoryService : IDisposable
{
    private bool _disposed = false;
    private readonly LdapConnection _connection;

    public string QueryBase { get; set; }

    public ActiveDirectoryService(ILdapConnectionFactory connectionFactory, ActiveDirectoryOptions options)
    {
        _connection = connectionFactory.CreateConnection();

        QueryBase = options.QueryBase;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _connection?.Dispose();
        }

        _disposed = true;
    }

    public SearchResponse SendSearchRequest(
        string distinguishedName,
        string filter,
        SearchScope searchScope,
        params string[] attributes)
    {
#if DEBUG
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
#if DEBUG
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

    public TUser? GetUser<TUser>(string[] attributes, string distinguishedName, string filter)
        where TUser : class, IUser<TUser>, new()
    {
        SearchResponse response = SendSearchRequest(
            distinguishedName,
            filter,
            SearchScope.Subtree,
            attributes);

        if (response.ResultCode != ResultCode.Success)
        {
            throw new SearchRequestException(response.ResultCode, response.ErrorMessage, response);
        }

        if (response.Entries.Count == 0)
        {
            return null;
        }

        if (response.Entries.Count > 1)
        {
            throw new InvalidOperationException("The input sequence contains more than one element.");
        }

        return TUser.ParseUser(response.Entries[0]);
    }

    public TUser? GetUserBySam<TUser>(string sAMAccountName, string[] attributes, string? queryBase = null)
        where TUser : class, IUser<TUser>, new()
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(sAMAccountName);

        string filter = new StringBuilder("(&")
            .Append("(objectCategory=person)")
            .Append("(objectClass=user)")
            .Append($"(sAMAccountName={sAMAccountName})")
            .Append(")")
            .ToString();

        return GetUser<TUser>(attributes, BuildDistinguishedName(queryBase), filter);
    }

    public TUser? GetUserByUpn<TUser>(string userPrincipalName, string[] attributes, string? queryBase = null)
        where TUser : class, IUser<TUser>, new()
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(userPrincipalName);

        string filter = new StringBuilder("(&")
            .Append("(objectCategory=person)")
            .Append("(objectClass=user)")
            .Append($"(userPrincipalName={userPrincipalName})")
            .Append(")")
            .ToString();

        return GetUser<TUser>(attributes, BuildDistinguishedName(queryBase), filter);
    }

    public TUser? GetUserByGuid<TUser>(string objectGuid, string[] attributes)
        where TUser : class, IUser<TUser>, new()
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(objectGuid);

        string distinguishedName = $"<GUID={objectGuid.ToUpper()}>";

        string filter = new StringBuilder("(&")
            .Append("(objectCategory=person)")
            .Append("(objectClass=user)")
            .Append(")")
            .ToString();

        return GetUser<TUser>(attributes, distinguishedName, filter);
    }

    public IEnumerable<TUser> GetUsers<TUser>(string[] attributes, string? filter = null, string? queryBase = null)
        where TUser : class, IUser<TUser>, new()
    {
        var filterBuilder = new StringBuilder("(&")
            .Append("(objectCategory=person)")
            .Append("(objectClass=user)");

        if (!string.IsNullOrEmpty(filter))
            filterBuilder.Append(filter);

        filterBuilder.Append(")")
            .ToString();

        SearchResponse response = SendSearchRequest(
            BuildDistinguishedName(queryBase),
            filterBuilder.ToString(),
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

    public void ChangePassword(string distinguishedName, string oldPassword, string newPassword, string? usersQueryBase = null)
    {
        ArgumentNullException.ThrowIfNull(distinguishedName);
        ArgumentNullException.ThrowIfNull(oldPassword);
        ArgumentNullException.ThrowIfNull(newPassword);

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

    /// <summary>
    /// Builds a distinguished name using the query base of the service
    /// and the query base within the current request.
    /// </summary>
    public string BuildDistinguishedName(string? queryBase = null)
    {
        if (!string.IsNullOrWhiteSpace(queryBase))
        {
            return $"{queryBase},{QueryBase}";
        }

        return QueryBase;
    }
}
