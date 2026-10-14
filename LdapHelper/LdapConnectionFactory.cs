using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;

using Microsoft.Extensions.Logging;

namespace LdapHelper;

public class LdapConnectionFactory : ILdapConnectionFactory
{
    private readonly LdapOptions _options;
    private readonly ILogger<LdapConnectionFactory>? _logger;

    public LdapOptions Options => _options;

    public LdapConnectionFactory(LdapOptions options, ILogger<LdapConnectionFactory>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    public static string NormalizeUsername(string username, string? domain)
    {
        ArgumentNullException.ThrowIfNull(username);

        if (string.IsNullOrWhiteSpace(domain) || username.Contains('\\') || username.Contains('@'))
            return username;

        return $@"{domain}\{username}";
    }

    public LdapConnection CreateConnection(
        string host,
        int port,
        string domain,
        string username,
        string password,
        bool secureSocketLayer = false,
        bool trustAllCertificates = false)
    {
        string normalizedUsername = NormalizeUsername(username, domain);

        _logger?.LogDebug(
            "Creating LDAP connection for user '{Username}' targeting {Host}:{Port} (SSL: {Ssl}, TrustAllCertificates: {TrustAll})",
            normalizedUsername,
            host,
            port,
            secureSocketLayer,
            trustAllCertificates);

        var identifier = new LdapDirectoryIdentifier(host, port);
        var credential = new NetworkCredential(normalizedUsername, password);

        var connection = new LdapConnection(identifier, credential, _options.AuthType)
        {
            AutoBind = true,
            Timeout = _options.Timeout
        };

        connection.SessionOptions.ProtocolVersion = _options.ProtocolVersion;
        connection.SessionOptions.SecureSocketLayer = secureSocketLayer;

        if (trustAllCertificates)
        {
            connection.SessionOptions.VerifyServerCertificate = static (_, _) => true;
        }

        return connection;
    }

    public LdapConnection CreateConnection() =>
        CreateConnection(
            _options.GetEffectiveHost(),
            _options.Port,
            _options.Domain,
            _options.Username,
            _options.Password,
            _options.SecureSocketLayer,
            _options.TrustAllCertificates);

    public LdapConnection CreateConnection(string username, string password) =>
        CreateConnection(
            _options.GetEffectiveHost(),
            _options.Port,
            _options.Domain,
            username,
            password,
            _options.SecureSocketLayer,
            _options.TrustAllCertificates);
}
