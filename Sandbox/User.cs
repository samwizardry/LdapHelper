using System.DirectoryServices.Protocols;
using System.Globalization;
using System.Text;

using LdapHelper;

namespace Sandbox;

class User : IUser<User>
{
    /// <summary>
    /// objectGUID.
    /// </summary>
    public Guid ObjectGuid { get; set; }

    /// <summary>
    /// sAMAccountName.
    /// </summary>
    public string SAMAccountName { get; set; } = null!;

    /// <summary>
    /// userPrincipalName.
    /// </summary>
    public string UserPrincipalName { get; set; } = null!;

    /// <summary>
    /// userAccountControl.
    /// </summary>
    public UserAccountControlFlag UserAccountControl { get; set; }

    /// <summary>
    /// whenCreated.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// whenChanged.
    /// </summary>
    public DateTimeOffset LastModifiedAt { get; set; }

    /// <summary>
    /// distinguishedName.
    /// </summary>
    public string DistinguishedName { get; set; } = null!;

    /// <summary>
    /// cn.
    /// </summary>
    public string? CommonName { get; set; }

    /// <summary>
    /// name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// memberOf.
    /// </summary>
    public List<string> Groups { get; set; } = [];

    /// <summary>
    /// mail.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// telephoneNumber.
    /// </summary>
    public string? TelephoneNumber { get; set; }

    /// <summary>
    /// givenName.
    /// </summary>
    public string? GivenName { get; set; }

    /// <summary>
    /// sn.
    /// </summary>
    public string? Surname { get; set; }

    /// <summary>
    /// company.
    /// </summary>
    public string? Company { get; set; }

    /// <summary>
    /// info.
    /// </summary>
    public string? Info { get; set; }

    /// <summary>
    /// logonCount.
    /// Counts the number of successful times the user tried to log on to this account.
    /// </summary>
    public int LogonCount { get; set; }

    /// <summary>
    /// lastLogon.
    /// The last time the user logged on.
    /// This value is stored as a large integer that represents the number of 100-nanosecond intervals since January 1, 1601 (UTC).
    /// A value of zero means that the last logon time is unknown.
    /// </summary>
    public long LastLogon { get; set; }

    /// <summary>
    /// lastLogonTimestamp.
    /// Lastlogon is only updated on the domain controller that performs the authentication and is not replicated.
    /// LastLogontimestamp is replicated, but by default only if it is 14 days or more older than the previous value.
    /// </summary>
    public long LastLogonTimestamp { get; set; }

    /// <summary>
    /// LastLogon в формате DateTimeOffset.
    /// </summary>
    public DateTimeOffset LastLogonDt { get; set; }

    /// <summary>
    /// LastLogonTimestamp в формате DateTimeOffset.
    /// </summary>
    public DateTimeOffset LastLogonTimestampDt { get; set; }

    public bool IsNormalAccount() =>
        (UserAccountControl & UserAccountControlFlag.NORMAL_ACCOUNT) == UserAccountControlFlag.NORMAL_ACCOUNT;

    public bool IsAccountDisabled() =>
        (UserAccountControl & UserAccountControlFlag.ACCOUNTDISABLE) == UserAccountControlFlag.ACCOUNTDISABLE;

    public bool IsLockedOut() =>
        (UserAccountControl & UserAccountControlFlag.LOCKOUT) == UserAccountControlFlag.LOCKOUT;

    public static User? ParseUser(SearchResultEntry entry)
    {
        var user = new User
        {
            ObjectGuid = new Guid(entry.GetAttributeValue<byte[]>("objectGUID")!),
            SAMAccountName = entry.GetAttributeValue<string>("sAMAccountName")!,
            UserPrincipalName = entry.GetAttributeValue<string>("userPrincipalName")!,
            UserAccountControl = (UserAccountControlFlag)int.Parse(entry.GetAttributeValue<string>("userAccountControl")!),
            CreatedAt = DateTimeOffset.ParseExact(entry.GetAttributeValue<string>("whenCreated")!, "yyyyMMddHHmmss.fZ", CultureInfo.InvariantCulture),
            LastModifiedAt = DateTimeOffset.ParseExact(entry.GetAttributeValue<string>("whenChanged")!, "yyyyMMddHHmmss.fZ", CultureInfo.InvariantCulture),
            DistinguishedName = entry.GetAttributeValue<string>("distinguishedName")!,
            CommonName = entry.GetAttributeValue<string>("cn"),
            Name = entry.GetAttributeValue<string>("name"),
            Email = entry.GetAttributeValue<string>("mail"),
            TelephoneNumber = entry.GetAttributeValue<string>("telephoneNumber"),
            GivenName = entry.GetAttributeValue<string>("givenName"),
            Surname = entry.GetAttributeValue<string>("sn"),
            Company = entry.GetAttributeValue<string>("company"),
            Info = entry.GetAttributeValue<string>("info"),
            LogonCount = int.TryParse(entry.GetAttributeValue<string>("logonCount"), out int lc) ? lc : 0,
            LastLogon = long.TryParse(entry.GetAttributeValue<string>("lastLogon"), out long ll) ? ll : 0,
            LastLogonTimestamp = long.TryParse(entry.GetAttributeValue<string>("lastLogonTimestamp"), out long llt) ? llt : 0,
            LastLogonDt = ll > 0 ? DateTimeOffset.FromFileTime(ll) : default,
            LastLogonTimestampDt = llt > 0 ? DateTimeOffset.FromFileTime(llt) : default
        };

        if (entry.Attributes["memberOf"] is not null)
        {
            var groups = entry.Attributes["memberOf"];

            foreach (var group in groups)
            {
                if (group is byte[] groupBytes)
                {
                    user.Groups.Add(Encoding.UTF8.GetString(groupBytes));
                }
            }
        }

        return user;
    }

    /// <summary>
    /// Requesting fields.
    /// </summary>
    public static readonly string[] Attributes = [
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
        "lastLogonTimestamp"];
}
