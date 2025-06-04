namespace LdapHelper;

public class LdapOptions
{
    /// <summary>
    /// Server address (e.g. 127.0.0.1).
    /// </summary>
    public string Host { get; init; } = null!;

    /// <summary>
    /// Server port (default: 389).
    /// </summary>
    public int Port { get; init; } = 389;

    /// <summary>
    /// Server zone (e.g. com).
    /// </summary>
    public string Zone { get; init; } = null!;

    /// <summary>
    /// Server domain (e.g. contoso).
    /// </summary>
    public string Domain { get; init; } = null!;

    /// <summary>
    /// Server subdomain (e.g. fabrikam).
    /// </summary>
    public string? Subdomain { get; init; }

    /// <summary>
    /// (e.g. fabrikam.contoso.com).
    /// </summary>
    public string Server { get; init; } = null!;

    /// <summary>
    /// This is for connecting via LDAPS (636 port).
    /// </summary>
    public bool SecureSocketLayer { get; init; } = false;

    /// <summary>
    /// Skip certificate verification.
    /// </summary>
    public bool TrustAllCertificates { get; init; } = false;

    /// <summary>
    /// Service account username.
    /// </summary>
    public string Username { get; init; } = null!;

    /// <summary>
    /// Service account password.
    /// </summary>
    public string Password { get; init; } = null!;
}
