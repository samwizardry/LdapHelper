using System;

namespace LdapHelper;

public static class Constants
{
    public const string BitAndMatchingRule = "1.2.840.113556.1.4.803";

    public const string ObjectCategoryPersonFilter = "objectCategory=person";
    public const string ObjectClassUserFilter = "objectClass=user";
    public const string AccountDisabledFilter = $"userAccountControl:{BitAndMatchingRule}:=2";
    public const string PasswordNeverExpiresFilter = $"userAccountControl:{BitAndMatchingRule}:=65536";
    public const string PasswordMarkedExpiredFilter = $"userAccountControl:{BitAndMatchingRule}:=8388608";
}
