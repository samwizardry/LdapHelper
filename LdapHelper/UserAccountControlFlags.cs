namespace LdapHelper;

/// <summary>
/// Active Directory userAccountControl attribute flags.
/// </summary>
[Flags]
public enum UserAccountControlFlags : int
{
    /// <summary>
    /// The logon script is executed.
    /// </summary>
    Script = 0x0001,

    /// <summary>
    /// The user account is disabled.
    /// </summary>
    AccountDisable = 0x0002,

    /// <summary>
    /// The home directory is required.
    /// </summary>
    HomeDirectoryRequired = 0x0008,

    /// <summary>
    /// The account is currently locked out.
    /// </summary>
    Lockout = 0x0010,

    /// <summary>
    /// No password is required.
    /// </summary>
    PasswordNotRequired = 0x0020,

    /// <summary>
    /// The user cannot change the password.
    /// </summary>
    PasswordCannotChange = 0x0040,

    /// <summary>
    /// The user can send an encrypted password.
    /// </summary>
    EncryptedTextPasswordAllowed = 0x0080,

    /// <summary>
    /// An account for users whose primary account is in another domain.
    /// </summary>
    TempDuplicateAccount = 0x0100,

    /// <summary>
    /// The default account type representing a normal user.
    /// </summary>
    NormalAccount = 0x0200,

    /// <summary>
    /// A permit to trust an account for a system domain that trusts other domains.
    /// </summary>
    InterdomainTrustAccount = 0x0800,

    /// <summary>
    /// A computer account for a computer that is running as a member workstation or server in this domain.
    /// </summary>
    WorkstationTrustAccount = 0x1000,

    /// <summary>
    /// A computer account for a domain controller that is a member of this domain.
    /// </summary>
    ServerTrustAccount = 0x2000,

    /// <summary>
    /// The password for this account never expires.
    /// </summary>
    DontExpirePassword = 0x10000,

    /// <summary>
    /// An MNS logon account.
    /// </summary>
    MnsLogonAccount = 0x20000,

    /// <summary>
    /// When set, the user is required to log on using a smart card.
    /// </summary>
    SmartcardRequired = 0x40000,

    /// <summary>
    /// When set, the service account is trusted for Kerberos delegation.
    /// </summary>
    TrustedForDelegation = 0x80000,

    /// <summary>
    /// When set, the security context of the user is not delegated to a service even if the service account is trusted.
    /// </summary>
    NotDelegated = 0x100000,

    /// <summary>
    /// Restrict this principal to use only Data Encryption Standard (DES) encryption types for keys.
    /// </summary>
    UseDesKeyOnly = 0x200000,

    /// <summary>
    /// This account does not require Kerberos pre-authentication for logging on.
    /// </summary>
    DontRequirePreauth = 0x400000,

    /// <summary>
    /// The user's password has expired.
    /// </summary>
    PasswordExpired = 0x800000,

    /// <summary>
    /// The account is enabled for delegation.
    /// </summary>
    TrustedToAuthForDelegation = 0x1000000,

    /// <summary>
    /// The account is a Read-Only Domain Controller (RODC).
    /// </summary>
    PartialSecretsAccount = 0x04000000
}
