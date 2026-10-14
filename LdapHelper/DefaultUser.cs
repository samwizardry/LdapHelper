using System.DirectoryServices.Protocols;

namespace LdapHelper;

public class DefaultUser : IUser<DefaultUser>
{
    /// <summary>
    /// objectGUID in Active Directory.
    /// </summary>
    public Guid? ObjectGuid { get; set; }

    /// <summary>
    /// sAMAccountName (Windows logon name).
    /// </summary>
    public string SamAccountName { get; set; } = string.Empty;

    /// <summary>
    /// userPrincipalName (e.g. user@domain.com). Can be null in Active Directory.
    /// </summary>
    public string? UserPrincipalName { get; set; }

    /// <summary>
    /// userAccountControl flags.
    /// </summary>
    public UserAccountControlFlags UserAccountControl { get; set; }

    /// <summary>
    /// whenCreated timestamp in UTC.
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// whenChanged timestamp in UTC.
    /// </summary>
    public DateTimeOffset? LastModifiedAt { get; set; }

    /// <summary>
    /// distinguishedName.
    /// </summary>
    public string DistinguishedName { get; set; } = string.Empty;

    /// <summary>
    /// cn (Common Name).
    /// </summary>
    public string? CommonName { get; set; }

    /// <summary>
    /// Display name (name attribute).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Distinguished names of groups the user belongs to (memberOf).
    /// </summary>
    public List<string> Groups { get; } = [];

    /// <summary>
    /// mail (Primary email address).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// telephoneNumber.
    /// </summary>
    public string? TelephoneNumber { get; set; }

    /// <summary>
    /// givenName (First name).
    /// </summary>
    public string? GivenName { get; set; }

    /// <summary>
    /// sn (Surname / Last name).
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// company.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// info (Notes/Comments).
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// logonCount. Number of successful logons to this account.
    /// </summary>
    public int LogonCount { get; set; }

    /// <summary>
    /// Raw lastLogon value (100-nanosecond intervals since January 1, 1601 UTC).
    /// Not replicated between domain controllers.
    /// </summary>
    public long LastLogon { get; set; }

    /// <summary>
    /// Raw lastLogonTimestamp value (replicated, updated at most every 9-14 days).
    /// </summary>
    public long LastLogonTimestamp { get; set; }

    /// <summary>
    /// lastLogon in UTC. Null if never logged on or unknown.
    /// </summary>
    public DateTimeOffset? LastLogonTime { get; set; }

    /// <summary>
    /// lastLogonTimestamp in UTC. Null if never logged on or unknown.
    /// </summary>
    public DateTimeOffset? LastLogonTimestampTime { get; set; }

    public bool IsNormalAccount() =>
        UserAccountControl.HasFlag(UserAccountControlFlags.NormalAccount);

    public bool IsAccountDisabled() =>
        UserAccountControl.HasFlag(UserAccountControlFlags.AccountDisable);

    public bool IsLockedOut() =>
        UserAccountControl.HasFlag(UserAccountControlFlags.Lockout);

    public bool DoesPasswordNeverExpire() =>
        UserAccountControl.HasFlag(UserAccountControlFlags.DontExpirePassword);

    public static DefaultUser? ParseUser(SearchResultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var user = new DefaultUser
        {
            ObjectGuid = entry.GetGuid("objectGUID"),
            SamAccountName = entry.GetString("sAMAccountName") ?? string.Empty,
            UserPrincipalName = entry.GetString("userPrincipalName"),
            UserAccountControl = (UserAccountControlFlags)(entry.GetInt32("userAccountControl") ?? 0),
            CreatedAt = entry.GetGeneralizedTime("whenCreated"),
            LastModifiedAt = entry.GetGeneralizedTime("whenChanged"),
            DistinguishedName = entry.GetString("distinguishedName") ?? entry.DistinguishedName,
            CommonName = entry.GetString("cn"),
            Name = entry.GetString("name"),
            Email = entry.GetString("mail"),
            TelephoneNumber = entry.GetString("telephoneNumber"),
            GivenName = entry.GetString("givenName"),
            Surname = entry.GetString("sn"),
            Company = entry.GetString("company"),
            Info = entry.GetString("info"),
            LogonCount = entry.GetInt32("logonCount") ?? 0,
            LastLogon = entry.GetInt64("lastLogon") ?? 0,
            LastLogonTimestamp = entry.GetInt64("lastLogonTimestamp") ?? 0,
            LastLogonTime = entry.GetFileTime("lastLogon"),
            LastLogonTimestampTime = entry.GetFileTime("lastLogonTimestamp")
        };

        foreach (string group in entry.GetStrings("memberOf"))
        {
            user.Groups.Add(group);
        }

        return user;
    }

    /// <summary>
    /// Default Active Directory attributes requested for DefaultUser.
    /// </summary>
    public static readonly string[] Attributes =
    [
        "objectGUID",
        "sAMAccountName",
        "userPrincipalName",
        "userAccountControl",
        "whenCreated",
        "whenChanged",
        "distinguishedName",
        "cn",
        "name",
        "memberOf",
        "mail",
        "telephoneNumber",
        "givenName",
        "sn",
        "company",
        "info",
        "logonCount",
        "lastLogon",
        "lastLogonTimestamp"
    ];
}
