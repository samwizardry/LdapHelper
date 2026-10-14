using Microsoft.Extensions.DependencyInjection;

namespace LdapHelper.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddLdapHelper_WithAction_RegistersServicesSuccessfully()
    {
        var services = new ServiceCollection();
        services.AddLdapHelper(options =>
        {
            options.Host = "127.0.0.1";
            options.Domain = "lorien.local";
        });

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<ILdapConnectionFactory>();
        Assert.NotNull(factory);
        Assert.Equal("127.0.0.1", factory.Options.Host);

        using IServiceScope scope = serviceProvider.CreateScope();
        var adService = scope.ServiceProvider.GetService<IActiveDirectoryService>();
        Assert.NotNull(adService);
    }

    [Fact]
    public void AddLdapHelper_WithOptionsInstance_RegistersServicesSuccessfully()
    {
        var services = new ServiceCollection();
        var options = new LdapOptions
        {
            Host = "10.0.0.1",
            Domain = "corp.local"
        };

        services.AddLdapHelper(options);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();

        var factory = serviceProvider.GetService<ILdapConnectionFactory>();
        Assert.NotNull(factory);
        Assert.Equal("10.0.0.1", factory.Options.Host);

        using IServiceScope scope = serviceProvider.CreateScope();
        var adService = scope.ServiceProvider.GetService<IActiveDirectoryService>();
        Assert.NotNull(adService);
    }
}
