using System.DirectoryServices.Protocols;
using System.Runtime.CompilerServices;
using System.Text;

using LdapHelper.Exceptions;

using Microsoft.Extensions.Logging;

namespace LdapHelper;

public class ActiveDirectoryService : IActiveDirectoryService
{
    private readonly LdapConnection _connection;
    private readonly ILdapConnectionFactory? _connectionFactory;
    private readonly string? _defaultQueryBase;
    private readonly bool _ownsConnection;
    private readonly ILogger<ActiveDirectoryService>? _logger;

    public ActiveDirectoryService(
        LdapConnection connection,
        string? defaultQueryBase = null,
        ILogger<ActiveDirectoryService>? logger = null)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _defaultQueryBase = defaultQueryBase;
        _logger = logger;
        _ownsConnection = false;
    }

    public ActiveDirectoryService(
        ILdapConnectionFactory connectionFactory,
        ILogger<ActiveDirectoryService>? logger = null)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _logger = logger;
        _connection = connectionFactory.CreateConnection();
        _defaultQueryBase = connectionFactory.Options.GetDefaultQueryBase();
        _ownsConnection = true;
    }

    public SearchResponse SendSearchRequest(
        string distinguishedName,
        string filter,
        SearchScope searchScope,
        params string[] attributes)
    {
        _logger?.LogDebug(
            "LDAP Search - Base: '{DistinguishedName}', Scope: {SearchScope}, Filter: {Filter}, Attributes: {Attributes}",
            distinguishedName,
            searchScope,
            filter,
            string.Join(", ", attributes));

        var request = new SearchRequest(distinguishedName, filter, searchScope, attributes);
        return (SearchResponse)_connection.SendRequest(request);
    }

    public async Task<SearchResponse> SendSearchRequestAsync(
        string distinguishedName,
        string filter,
        SearchScope searchScope,
        string[] attributes,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.Run(() => SendSearchRequest(distinguishedName, filter, searchScope, attributes), cancellationToken)
            .ConfigureAwait(false);
    }

    public ModifyResponse SendModifyRequest(
        string distinguishedName,
        params DirectoryAttributeModification[] modifications)
    {
        if (_logger?.IsEnabled(LogLevel.Debug) == true)
        {
            var modSummary = modifications.Select(m => $"{m.Operation}:{m.Name}");
            _logger.LogDebug("LDAP Modify - DN: '{DistinguishedName}', Operations: [{Operations}]", distinguishedName, string.Join("; ", modSummary));
        }

        var request = new ModifyRequest(distinguishedName, modifications);
        return (ModifyResponse)_connection.SendRequest(request);
    }

    public async Task<ModifyResponse> SendModifyRequestAsync(
        string distinguishedName,
        DirectoryAttributeModification[] modifications,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.Run(() => SendModifyRequest(distinguishedName, modifications), cancellationToken)
            .ConfigureAwait(false);
    }

    public IEnumerable<TUser> GetUsers<TUser>(
        string[] attributes,
        string queryBase,
        string filter,
        int pageSize = 1000)
        where TUser : class, IUser<TUser>
    {
        ArgumentNullException.ThrowIfNull(attributes);
        ArgumentException.ThrowIfNullOrWhiteSpace(queryBase);
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        var request = new SearchRequest(queryBase, filter, SearchScope.Subtree, attributes);

        PageResultRequestControl? pageRequestControl = null;
        if (pageSize > 0)
        {
            pageRequestControl = new PageResultRequestControl(pageSize);
            request.Controls.Add(pageRequestControl);
        }

        while (true)
        {
            SearchResponse response = (SearchResponse)_connection.SendRequest(request);

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

            if (pageRequestControl is null)
                break;

            PageResultResponseControl? pageResponseControl = null;
            foreach (DirectoryControl control in response.Controls)
            {
                if (control is PageResultResponseControl prc)
                {
                    pageResponseControl = prc;
                    break;
                }
            }

            if (pageResponseControl is null || pageResponseControl.Cookie is null || pageResponseControl.Cookie.Length == 0)
                break;

            pageRequestControl.Cookie = pageResponseControl.Cookie;
        }
    }

    public async IAsyncEnumerable<TUser> GetUsersAsync<TUser>(
        string[] attributes,
        string queryBase,
        string filter,
        int pageSize = 1000,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>
    {
        ArgumentNullException.ThrowIfNull(attributes);
        ArgumentException.ThrowIfNullOrWhiteSpace(queryBase);
        ArgumentException.ThrowIfNullOrWhiteSpace(filter);

        var request = new SearchRequest(queryBase, filter, SearchScope.Subtree, attributes);

        PageResultRequestControl? pageRequestControl = null;
        if (pageSize > 0)
        {
            pageRequestControl = new PageResultRequestControl(pageSize);
            request.Controls.Add(pageRequestControl);
        }

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            SearchResponse response = await Task.Run(() => (SearchResponse)_connection.SendRequest(request), cancellationToken)
                .ConfigureAwait(false);

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

            if (pageRequestControl is null)
                break;

            PageResultResponseControl? pageResponseControl = null;
            foreach (DirectoryControl control in response.Controls)
            {
                if (control is PageResultResponseControl prc)
                {
                    pageResponseControl = prc;
                    break;
                }
            }

            if (pageResponseControl is null || pageResponseControl.Cookie is null || pageResponseControl.Cookie.Length == 0)
                break;

            pageRequestControl.Cookie = pageResponseControl.Cookie;
        }
    }

    public TUser? GetUserBySam<TUser>(
        string samAccountName,
        string? queryBase = null,
        string[]? attributes = null)
        where TUser : class, IUser<TUser>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(samAccountName);

        string effectiveBase = ResolveQueryBase(queryBase);
        string filter = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass()
            .AddSamAccountName(samAccountName)
            .Build();

        return GetUsers<TUser>(attributes ?? DefaultUser.Attributes, effectiveBase, filter, pageSize: 1)
            .FirstOrDefault();
    }

    public async Task<TUser?> GetUserBySamAsync<TUser>(
        string samAccountName,
        string? queryBase = null,
        string[]? attributes = null,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(samAccountName);

        string effectiveBase = ResolveQueryBase(queryBase);
        string filter = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass()
            .AddSamAccountName(samAccountName)
            .Build();

        await foreach (TUser user in GetUsersAsync<TUser>(attributes ?? DefaultUser.Attributes, effectiveBase, filter, pageSize: 1, cancellationToken).ConfigureAwait(false))
        {
            return user;
        }

        return null;
    }

    public TUser? GetUserByUpn<TUser>(
        string upn,
        string? queryBase = null,
        string[]? attributes = null)
        where TUser : class, IUser<TUser>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        string effectiveBase = ResolveQueryBase(queryBase);
        string filter = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass()
            .AddUpn(upn)
            .Build();

        return GetUsers<TUser>(attributes ?? DefaultUser.Attributes, effectiveBase, filter, pageSize: 1)
            .FirstOrDefault();
    }

    public async Task<TUser?> GetUserByUpnAsync<TUser>(
        string upn,
        string? queryBase = null,
        string[]? attributes = null,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(upn);

        string effectiveBase = ResolveQueryBase(queryBase);
        string filter = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass()
            .AddUpn(upn)
            .Build();

        await foreach (TUser user in GetUsersAsync<TUser>(attributes ?? DefaultUser.Attributes, effectiveBase, filter, pageSize: 1, cancellationToken).ConfigureAwait(false))
        {
            return user;
        }

        return null;
    }

    public IEnumerable<TUser> GetUsersByGroup<TUser>(
        string groupDistinguishedName,
        string? queryBase = null,
        string[]? attributes = null,
        int pageSize = 1000)
        where TUser : class, IUser<TUser>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupDistinguishedName);

        string effectiveBase = ResolveQueryBase(queryBase);
        string filter = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass()
            .AddMemberOf(groupDistinguishedName)
            .Build();

        return GetUsers<TUser>(attributes ?? DefaultUser.Attributes, effectiveBase, filter, pageSize);
    }

    public IAsyncEnumerable<TUser> GetUsersByGroupAsync<TUser>(
        string groupDistinguishedName,
        string? queryBase = null,
        string[]? attributes = null,
        int pageSize = 1000,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(groupDistinguishedName);

        string effectiveBase = ResolveQueryBase(queryBase);
        string filter = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass()
            .AddMemberOf(groupDistinguishedName)
            .Build();

        return GetUsersAsync<TUser>(attributes ?? DefaultUser.Attributes, effectiveBase, filter, pageSize, cancellationToken);
    }

    public void ChangePassword(string distinguishedName, string oldPassword, string newPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(distinguishedName);
        ArgumentNullException.ThrowIfNull(oldPassword);
        ArgumentNullException.ThrowIfNull(newPassword);

        var removePwdMod = new DirectoryAttributeModification
        {
            Operation = DirectoryAttributeOperation.Delete,
            Name = "unicodePwd"
        };
        removePwdMod.Add(Encoding.Unicode.GetBytes($"\"{oldPassword}\""));

        var addPwdMod = new DirectoryAttributeModification
        {
            Operation = DirectoryAttributeOperation.Add,
            Name = "unicodePwd"
        };
        addPwdMod.Add(Encoding.Unicode.GetBytes($"\"{newPassword}\""));

        var response = SendModifyRequest(distinguishedName, removePwdMod, addPwdMod);

        if (response.ResultCode != ResultCode.Success)
        {
            throw new ModifyRequestException(response.ResultCode, response.ErrorMessage, response);
        }
    }

    public async Task ChangePasswordAsync(string distinguishedName, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Run(() => ChangePassword(distinguishedName, oldPassword, newPassword), cancellationToken)
            .ConfigureAwait(false);
    }

    public void ResetPassword(string distinguishedName, string newPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(distinguishedName);
        ArgumentNullException.ThrowIfNull(newPassword);

        var replacePwdMod = new DirectoryAttributeModification
        {
            Operation = DirectoryAttributeOperation.Replace,
            Name = "unicodePwd"
        };
        replacePwdMod.Add(Encoding.Unicode.GetBytes($"\"{newPassword}\""));

        var response = SendModifyRequest(distinguishedName, replacePwdMod);

        if (response.ResultCode != ResultCode.Success)
        {
            throw new ModifyRequestException(response.ResultCode, response.ErrorMessage, response);
        }
    }

    public async Task ResetPasswordAsync(string distinguishedName, string newPassword, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Run(() => ResetPassword(distinguishedName, newPassword), cancellationToken)
            .ConfigureAwait(false);
    }

    public void Authenticate(string username, string password)
    {
        if (_connectionFactory is null)
            throw new InvalidOperationException("No connection factory was configured for this ActiveDirectoryService instance.");

        Authenticate(_connectionFactory, username, password);
    }

    public void Authenticate(ILdapConnectionFactory connectionFactory, string username, string password)
    {
        ArgumentNullException.ThrowIfNull(connectionFactory);
        ArgumentNullException.ThrowIfNull(username);
        ArgumentNullException.ThrowIfNull(password);

        using var connection = connectionFactory.CreateConnection(username, password);
        connection.Bind();
    }

    public bool TryAuthenticate(string username, string password, out string? errorMessage)
    {
        if (_connectionFactory is null)
            throw new InvalidOperationException("No connection factory was configured for this ActiveDirectoryService instance.");

        return TryAuthenticate(_connectionFactory, username, password, out errorMessage);
    }

    public bool TryAuthenticate(string username, string password) =>
        TryAuthenticate(username, password, out _);

    public bool TryAuthenticate(ILdapConnectionFactory connectionFactory, string username, string password, out string? errorMessage)
    {
        errorMessage = null;
        try
        {
            Authenticate(connectionFactory, username, password);
            return true;
        }
        catch (LdapException ex)
        {
            errorMessage = ex.Message;
            _logger?.LogWarning(ex, "LDAP authentication failed for user '{Username}'. Result code: {ErrorCode}", username, ex.ErrorCode);
            return false;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
            _logger?.LogError(ex, "Unexpected error authenticating user '{Username}'", username);
            return false;
        }
    }

    public bool TryAuthenticate(ILdapConnectionFactory connectionFactory, string username, string password) =>
        TryAuthenticate(connectionFactory, username, password, out _);

    public async Task<bool> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.Run(() => TryAuthenticate(username, password), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> TryAuthenticateAsync(ILdapConnectionFactory connectionFactory, string username, string password, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return await Task.Run(() => TryAuthenticate(connectionFactory, username, password), cancellationToken)
            .ConfigureAwait(false);
    }

    public void Dispose()
    {
        if (_ownsConnection)
        {
            _connection.Dispose();
        }
    }

    private string ResolveQueryBase(string? queryBase)
    {
        if (!string.IsNullOrWhiteSpace(queryBase))
            return queryBase;

        if (!string.IsNullOrWhiteSpace(_defaultQueryBase))
            return _defaultQueryBase;

        throw new InvalidOperationException(
            "No queryBase was provided and no default query base was configured in LdapOptions or constructor.");
    }
}
