namespace LdapHelper.Tests;

public class FilterBuilderTests
{
    [Fact]
    public void Build_WhenEmpty_ReturnsWildcardObjectFilter()
    {
        var builder = new FilterBuilder();

        string filter = builder.Build();

        Assert.Equal("(&(objectClass=*))", filter);
    }

    [Fact]
    public void Build_WhenCalledMultipleTimes_ProducesSameResultWithoutMutatingState()
    {
        var builder = new FilterBuilder()
            .AddObjectCategory()
            .AddObjectClass();

        string filter1 = builder.Build();
        string filter2 = builder.Build();

        Assert.Equal("(&(objectCategory=person)(objectClass=user))", filter1);
        Assert.Equal(filter1, filter2);
    }

    [Fact]
    public void AddEquals_EscapesSpecialLdapCharacters()
    {
        var builder = new FilterBuilder()
            .AddEquals("cn", @"John (Dev)*\Smith");

        string filter = builder.Build();

        Assert.Equal(@"(&(cn=John \28Dev\29\2a\5cSmith))", filter);
    }

    [Fact]
    public void AddMemberOf_WithMultipleGroups_GeneratesOrBlock()
    {
        var builder = new FilterBuilder()
            .AddMemberOf("CN=Admins,DC=corp", "CN=Users,DC=corp");

        string filter = builder.Build();

        Assert.Equal("(&(|(memberOf=CN=Admins,DC=corp)(memberOf=CN=Users,DC=corp)))", filter);
    }

    [Fact]
    public void AddMemberOf_WithEmptyArray_DoesNotGenerateInvalidFilter()
    {
        var builder = new FilterBuilder()
            .AddObjectClass()
            .AddMemberOf();

        string filter = builder.Build();

        Assert.Equal("(&(objectClass=user))", filter);
    }

    [Fact]
    public void AddAccountEnabledFilter_GeneratesNegatedAccountDisabled()
    {
        var builder = new FilterBuilder()
            .AddAccountEnabledFilter();

        string filter = builder.Build();

        Assert.Equal($"(&(!({Constants.AccountDisabledFilter})))", filter);
    }
}
