using System.DirectoryServices.Protocols;
using System.Net;
using System.Security.Cryptography.X509Certificates;

namespace LdapHelper;

public class LdapConnectionFactory : ILdapConnectionFactory
{
    private readonly LdapOptions _options;

    public LdapConnectionFactory(LdapOptions options)
    {
        _options = options;
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
        // On Windows the authentication type is Negotiate, so there is no need to prepend
        // AD user login with domain. On other platforms at the moment only
        // Basic authentication is supported

        // Also can fail on non AD servers, so you might prefer
        // to just use AuthType.Basic everywhere

        if (username.IndexOf('\\') == -1)
            username = $@"{domain}\{username}";

        // Depending on LDAP server, username might require some proper wrapping
        // instead(!) of prepending username with domain
        //username = $"uid={username},CN=Users,DC={options.Subdomain},DC={options.Domain},DC={options.Zone}";

#if DEBUG
        Console.WriteLine($"Creating LDAP Connection for {username} with Directory Identifier {host}:{port} and AuthType {AuthType.Basic}");
#endif

        var connection = new LdapConnection(new LdapDirectoryIdentifier(host, port))
        {
            AuthType = AuthType.Basic,
            Credential = new NetworkCredential(username, password),
            AutoBind = true
        };

        // The default one is v2 (at least in that version), and it is unknown if v3
        // is actually needed, but at least Synology LDAP works only with v3,
        // and since our Exchange doesn't complain, let it be v3
        connection.SessionOptions.ProtocolVersion = 3;

        // This is for connecting via LDAPS (636 port). It should be working,
        // according to https://github.com/dotnet/runtime/issues/43890
        connection.SessionOptions.SecureSocketLayer = secureSocketLayer;

        if (trustAllCertificates)
        {
            connection.SessionOptions.VerifyServerCertificate += delegate (LdapConnection connection, X509Certificate certificate)
            {
                return true;
            };
        }

        return connection;
    }

    public LdapConnection CreateConnection() =>
        CreateConnection(
            _options.Host,
            _options.Port,
            _options.Domain,
            _options.Username,
            _options.Password,
            _options.SecureSocketLayer,
            _options.TrustAllCertificates);

    public LdapConnection CreateConnection(string username, string password) =>
        CreateConnection(
            _options.Host,
            _options.Port,
            _options.Domain,
            username,
            password,
            _options.SecureSocketLayer,
            _options.TrustAllCertificates);
}
