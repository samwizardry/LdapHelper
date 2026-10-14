namespace LdapHelper;

public static class Constants
{
    public const string BitAndMatchingRule = "1.2.840.113556.1.4.803";
    public const string BitOrMatchingRule = "1.2.840.113556.1.4.804";

    public const string ObjectCategoryPersonFilter = "objectCategory=person";
    public const string ObjectClassUserFilter = "objectClass=user";

    public static readonly string AccountDisabledFilter = $"userAccountControl:{BitAndMatchingRule}:={(int)UserAccountControlFlags.AccountDisable}";
    public static readonly string PasswordNeverExpiresFilter = $"userAccountControl:{BitAndMatchingRule}:={(int)UserAccountControlFlags.DontExpirePassword}";
    public static readonly string PasswordMarkedExpiredFilter = $"userAccountControl:{BitAndMatchingRule}:={(int)UserAccountControlFlags.PasswordExpired}";
}
