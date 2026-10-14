namespace LdapHelper.Tests;

public class DefaultUserTests
{
    [Fact]
    public void AccountStatusChecks_WorkCorrectly()
    {
        var user = new DefaultUser
        {
            UserAccountControl = UserAccountControlFlags.NormalAccount | UserAccountControlFlags.DontExpirePassword
        };

        Assert.True(user.IsNormalAccount());
        Assert.False(user.IsAccountDisabled());
        Assert.False(user.IsLockedOut());
        Assert.True(user.DoesPasswordNeverExpire());

        user.UserAccountControl |= UserAccountControlFlags.AccountDisable;
        Assert.True(user.IsAccountDisabled());

        user.UserAccountControl |= UserAccountControlFlags.Lockout;
        Assert.True(user.IsLockedOut());
    }
}
