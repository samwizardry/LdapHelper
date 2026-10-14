namespace LdapHelper.Tests;

public class LdapConnectionFactoryTests
{
    [Theory]
    [InlineData("john", "lorien", @"lorien\john")]
    [InlineData(@"lorien\john", "lorien", @"lorien\john")]
    [InlineData(@"corp\john", "lorien", @"corp\john")]
    [InlineData("john@lorien.local", "lorien", "john@lorien.local")]
    [InlineData("john", null, "john")]
    [InlineData("john", "", "john")]
    public void NormalizeUsername_FormatsCorrectly(string username, string? domain, string expected)
    {
        string actual = LdapConnectionFactory.NormalizeUsername(username, domain);

        Assert.Equal(expected, actual);
    }
}
