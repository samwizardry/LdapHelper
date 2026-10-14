using System.DirectoryServices.Protocols;

namespace LdapHelper;

public class LdapOptions
{
    /// <summary>
    /// Server host or IP address (e.g. 192.168.0.137 or dc1.lorien.local).
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// Server port (default: 389 for plain LDAP, 636 for LDAPS).
    /// </summary>
    public int Port { get; set; } = 389;

    /// <summary>
    /// Domain name (e.g. "lorien" for NetBIOS domain or "lorien.local" for DNS domain).
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Optional server DNS name (e.g. "lorien.local"). Falls back to Host if not set.
    /// </summary>
    public string? Server { get; set; }

    /// <summary>
    /// Optional top-level zone (e.g. "local" or "com").
    /// </summary>
    public string? Zone { get; set; }

    /// <summary>
    /// Optional subdomain (e.g. "corp").
    /// </summary>
    public string? Subdomain { get; set; }

    /// <summary>
    /// Connect via SSL/TLS (LDAPS, usually on port 636).
    /// Required for password changes and resets in Active Directory.
    /// </summary>
    public bool SecureSocketLayer { get; set; } = false;

    /// <summary>
    /// Skip SSL certificate validation (warning: use only for local testing/development).
    /// </summary>
    public bool TrustAllCertificates { get; set; } = false;

    /// <summary>
    /// Service account username (can be 'user', 'DOMAIN\user', or 'user@domain.local').
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Service account password.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// LDAP protocol version (default: 3).
    /// </summary>
    public int ProtocolVersion { get; set; } = 3;

    /// <summary>
    /// Authentication type (default: Basic).
    /// </summary>
    public AuthType AuthType { get; set; } = AuthType.Basic;

    /// <summary>
    /// Connection timeout.
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Default query base DN (e.g. "DC=lorien,DC=local"). If null, calculated from Domain/Zone/Server.
    /// </summary>
    public string? DefaultQueryBase { get; set; }

    /// <summary>
    /// Resolves the effective target host name.
    /// </summary>
    public string GetEffectiveHost() =>
        !string.IsNullOrWhiteSpace(Host) ? Host : (Server ?? string.Empty);

    /// <summary>
    /// Resolves the default query base distinguished name.
    /// </summary>
    public string GetDefaultQueryBase()
    {
        if (!string.IsNullOrWhiteSpace(DefaultQueryBase))
            return DefaultQueryBase;

        string domainName = !string.IsNullOrWhiteSpace(Domain) ? Domain : (Server ?? Host);
        if (domainName.Contains('.'))
        {
            return string.Join(",", domainName.Split('.', StringSplitOptions.RemoveEmptyEntries).Select(part => $"DC={part}"));
        }

        if (!string.IsNullOrWhiteSpace(Zone))
        {
            return $"DC={domainName},DC={Zone}";
        }

        return string.Empty;
    }
}
