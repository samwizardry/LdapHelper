namespace LdapHelper.Tests;

public class ActiveDirectoryIntegrationTests
{
    private static LdapOptions CreateTestOptions() => new()
    {
        Host = "192.168.0.137",
        Zone = "local",
        Domain = "lorien",
        Server = "lorien.local",
        Username = "Administrator",
        Password = "P@ssw0rd",
        DefaultQueryBase = "OU=Company Users,DC=lorien,DC=local"
    };

    private static (ActiveDirectoryService Service, ILdapConnectionFactory Factory) CreateService()
    {
        var options = CreateTestOptions();
        var factory = new LdapConnectionFactory(options);
        var service = new ActiveDirectoryService(factory);
        return (service, factory);
    }

    [Fact]
    public void GetUserByUpn_FindsJohnSuccessfully()
    {
        var (service, _) = CreateService();
        using (service)
        {
            DefaultUser? john = service.GetUserByUpn<DefaultUser>("john@lorien.local");

            Assert.NotNull(john);
            Assert.Equal("john", john.SamAccountName);
            Assert.Equal("john@lorien.local", john.UserPrincipalName);
            Assert.Contains("CN=john", john.DistinguishedName);
            Assert.True(john.IsNormalAccount());
        }
    }

    [Fact]
    public async Task GetUserByUpnAsync_FindsJohnSuccessfully()
    {
        var (service, _) = CreateService();
        using (service)
        {
            DefaultUser? john = await service.GetUserByUpnAsync<DefaultUser>(
                "john@lorien.local",
                cancellationToken: TestContext.Current.CancellationToken);

            Assert.NotNull(john);
            Assert.Equal("john", john.SamAccountName);
            Assert.Equal("john@lorien.local", john.UserPrincipalName);
        }
    }

    [Fact]
    public void GetUserBySam_WhenUserExists_ReturnsUser()
    {
        var (service, _) = CreateService();
        using (service)
        {
            DefaultUser? user = service.GetUserBySam<DefaultUser>("john");

            Assert.NotNull(user);
            Assert.Equal("john", user.SamAccountName);
        }
    }

    [Fact]
    public void GetUsersByGroup_ReturnsManagers()
    {
        var (service, _) = CreateService();
        using (service)
        {
            const string groupDn = "CN=Company Managers,OU=Company Groups,DC=lorien,DC=local";
            List<DefaultUser> managers = service.GetUsersByGroup<DefaultUser>(groupDn).ToList();

            Assert.NotEmpty(managers);
            Assert.Contains(managers, m => m.UserPrincipalName == "john@lorien.local");
        }
    }

    [Fact]
    public async Task GetUsersByGroupAsync_StreamsManagers()
    {
        var (service, _) = CreateService();
        using (service)
        {
            const string groupDn = "CN=Company Managers,OU=Company Groups,DC=lorien,DC=local";
            var managers = new List<DefaultUser>();

            await foreach (DefaultUser manager in service.GetUsersByGroupAsync<DefaultUser>(
                groupDn,
                cancellationToken: TestContext.Current.CancellationToken))
            {
                managers.Add(manager);
            }

            Assert.NotEmpty(managers);
            Assert.Contains(managers, m => m.UserPrincipalName == "john@lorien.local");
        }
    }

    [Fact]
    public void GetUsers_WithFilterBuilder_ReturnsFilteredUsers()
    {
        var (service, factory) = CreateService();
        using (service)
        {
            string filter = new FilterBuilder()
                .AddObjectCategory()
                .AddObjectClass()
                .AddAccountEnabledFilter()
                .AddUpn("john@lorien.local")
                .Build();

            List<DefaultUser> users = service.GetUsers<DefaultUser>(
                DefaultUser.Attributes,
                factory.Options.GetDefaultQueryBase(),
                filter).ToList();

            Assert.NotEmpty(users);
            Assert.Contains(users, u => u.SamAccountName == "john");
        }
    }

    [Fact]
    public void TryAuthenticate_WithValidCredentials_ReturnsTrue()
    {
        var (service, factory) = CreateService();
        using (service)
        {
            bool success = service.TryAuthenticate(factory, "Administrator", "P@ssw0rd", out string? errorMessage);

            Assert.True(success);
            Assert.Null(errorMessage);
        }
    }

    [Fact]
    public void TryAuthenticate_WithInvalidCredentials_ReturnsFalseAndError()
    {
        var (service, factory) = CreateService();
        using (service)
        {
            bool success = service.TryAuthenticate(factory, "Administrator", "Invalid_Password_12345", out string? errorMessage);

            Assert.False(success);
            Assert.NotNull(errorMessage);
        }
    }
}
