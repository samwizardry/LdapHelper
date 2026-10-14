namespace LdapHelper.Tests;

public class LdapOptionsTests
{
    [Fact]
    public void GetDefaultQueryBase_WhenExplicitlySet_ReturnsExplicitValue()
    {
        var options = new LdapOptions
        {
            DefaultQueryBase = "OU=Company Users,DC=lorien,DC=local"
        };

        Assert.Equal("OU=Company Users,DC=lorien,DC=local", options.GetDefaultQueryBase());
    }

    [Theory]
    [InlineData("lorien.local", null, "DC=lorien,DC=local")]
    [InlineData("lorien", "local", "DC=lorien,DC=local")]
    [InlineData("corp.contoso.com", null, "DC=corp,DC=contoso,DC=com")]
    public void GetDefaultQueryBase_CalculatesFromDomainAndZone(string domain, string? zone, string expected)
    {
        var options = new LdapOptions
        {
            Domain = domain,
            Zone = zone
        };

        Assert.Equal(expected, options.GetDefaultQueryBase());
    }
}
