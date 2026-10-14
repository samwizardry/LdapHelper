using System.DirectoryServices.Protocols;

namespace LdapHelper;

public interface IActiveDirectoryService : IDisposable
{
    /// <summary>
    /// Executes a raw LDAP search request.
    /// </summary>
    SearchResponse SendSearchRequest(
        string distinguishedName,
        string filter,
        SearchScope searchScope,
        params string[] attributes);

    /// <summary>
    /// Executes an asynchronous raw LDAP search request.
    /// </summary>
    Task<SearchResponse> SendSearchRequestAsync(
        string distinguishedName,
        string filter,
        SearchScope searchScope,
        string[] attributes,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a raw LDAP modification request.
    /// </summary>
    ModifyResponse SendModifyRequest(
        string distinguishedName,
        params DirectoryAttributeModification[] modifications);

    /// <summary>
    /// Executes an asynchronous raw LDAP modification request.
    /// </summary>
    Task<ModifyResponse> SendModifyRequestAsync(
        string distinguishedName,
        DirectoryAttributeModification[] modifications,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for users matching the given filter with automatic LDAP pagination.
    /// </summary>
    IEnumerable<TUser> GetUsers<TUser>(
        string[] attributes,
        string queryBase,
        string filter,
        int pageSize = 1000)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Asynchronously searches for users matching the given filter with automatic LDAP pagination.
    /// </summary>
    IAsyncEnumerable<TUser> GetUsersAsync<TUser>(
        string[] attributes,
        string queryBase,
        string filter,
        int pageSize = 1000,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Retrieves a single user by sAMAccountName (Windows logon name).
    /// </summary>
    TUser? GetUserBySam<TUser>(
        string samAccountName,
        string? queryBase = null,
        string[]? attributes = null)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Asynchronously retrieves a single user by sAMAccountName (Windows logon name).
    /// </summary>
    Task<TUser?> GetUserBySamAsync<TUser>(
        string samAccountName,
        string? queryBase = null,
        string[]? attributes = null,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Retrieves a single DefaultUser by sAMAccountName (Windows logon name).
    /// </summary>
    DefaultUser? GetUserBySam(
        string samAccountName,
        string? queryBase = null,
        string[]? attributes = null);

    /// <summary>
    /// Asynchronously retrieves a single DefaultUser by sAMAccountName (Windows logon name).
    /// </summary>
    Task<DefaultUser?> GetUserBySamAsync(
        string samAccountName,
        string? queryBase = null,
        string[]? attributes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a single user by userPrincipalName (UPN).
    /// </summary>
    TUser? GetUserByUpn<TUser>(
        string upn,
        string? queryBase = null,
        string[]? attributes = null)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Asynchronously retrieves a single user by userPrincipalName (UPN).
    /// </summary>
    Task<TUser?> GetUserByUpnAsync<TUser>(
        string upn,
        string? queryBase = null,
        string[]? attributes = null,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Retrieves a single DefaultUser by userPrincipalName (UPN).
    /// </summary>
    DefaultUser? GetUserByUpn(
        string upn,
        string? queryBase = null,
        string[]? attributes = null);

    /// <summary>
    /// Asynchronously retrieves a single DefaultUser by userPrincipalName (UPN).
    /// </summary>
    Task<DefaultUser?> GetUserByUpnAsync(
        string upn,
        string? queryBase = null,
        string[]? attributes = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all users that are members of the specified group distinguished name.
    /// </summary>
    IEnumerable<TUser> GetUsersByGroup<TUser>(
        string groupDistinguishedName,
        string? queryBase = null,
        string[]? attributes = null,
        int pageSize = 1000)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Asynchronously retrieves all users that are members of the specified group distinguished name.
    /// </summary>
    IAsyncEnumerable<TUser> GetUsersByGroupAsync<TUser>(
        string groupDistinguishedName,
        string? queryBase = null,
        string[]? attributes = null,
        int pageSize = 1000,
        CancellationToken cancellationToken = default)
        where TUser : class, IUser<TUser>;

    /// <summary>
    /// Retrieves all DefaultUser instances that are members of the specified group distinguished name.
    /// </summary>
    IEnumerable<DefaultUser> GetUsersByGroup(
        string groupDistinguishedName,
        string? queryBase = null,
        string[]? attributes = null,
        int pageSize = 1000);

    /// <summary>
    /// Asynchronously retrieves all DefaultUser instances that are members of the specified group distinguished name.
    /// </summary>
    IAsyncEnumerable<DefaultUser> GetUsersByGroupAsync(
        string groupDistinguishedName,
        string? queryBase = null,
        string[]? attributes = null,
        int pageSize = 1000,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Changes an Active Directory user password with verification of the old password.
    /// Requires LDAPS (SSL/TLS).
    /// </summary>
    void ChangePassword(string distinguishedName, string oldPassword, string newPassword);

    /// <summary>
    /// Asynchronously changes an Active Directory user password with verification of the old password.
    /// Requires LDAPS (SSL/TLS).
    /// </summary>
    Task ChangePasswordAsync(string distinguishedName, string oldPassword, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets an Active Directory user password without requiring the old password (administrative reset).
    /// Requires LDAPS (SSL/TLS) and appropriate administrative permissions.
    /// </summary>
    void ResetPassword(string distinguishedName, string newPassword);

    /// <summary>
    /// Asynchronously resets an Active Directory user password without requiring the old password (administrative reset).
    /// Requires LDAPS (SSL/TLS) and appropriate administrative permissions.
    /// </summary>
    Task ResetPasswordAsync(string distinguishedName, string newPassword, CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates user credentials using the service's configured connection factory.
    /// </summary>
    void Authenticate(string username, string password);

    /// <summary>
    /// Authenticates user credentials by attempting to bind against the LDAP server.
    /// </summary>
    void Authenticate(ILdapConnectionFactory connectionFactory, string username, string password);

    /// <summary>
    /// Checks user credentials using the service's configured connection factory.
    /// </summary>
    bool TryAuthenticate(string username, string password, out string? errorMessage);

    /// <summary>
    /// Checks user credentials using the service's configured connection factory.
    /// </summary>
    bool TryAuthenticate(string username, string password);

    /// <summary>
    /// Checks user credentials. Returns true if credentials are valid, false otherwise.
    /// </summary>
    bool TryAuthenticate(ILdapConnectionFactory connectionFactory, string username, string password, out string? errorMessage);

    /// <summary>
    /// Checks user credentials. Returns true if credentials are valid, false otherwise.
    /// </summary>
    bool TryAuthenticate(ILdapConnectionFactory connectionFactory, string username, string password);

    /// <summary>
    /// Asynchronously checks user credentials.
    /// </summary>
    Task<bool> TryAuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously checks user credentials.
    /// </summary>
    Task<bool> TryAuthenticateAsync(ILdapConnectionFactory connectionFactory, string username, string password, CancellationToken cancellationToken = default);
}
