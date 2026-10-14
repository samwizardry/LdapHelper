using LdapHelper;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.Extensions.DependencyInjection;

public static class LdapServiceCollectionExtensions
{
    /// <summary>
    /// Adds LdapHelper services (ILdapConnectionFactory, IActiveDirectoryService) to the specified IServiceCollection.
    /// </summary>
    public static IServiceCollection AddLdapHelper(
        this IServiceCollection services,
        Action<LdapOptions> configureOptions)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configureOptions);

        services.Configure(configureOptions);

        services.AddSingleton<ILdapConnectionFactory>(static sp =>
        {
            LdapOptions options = sp.GetRequiredService<IOptions<LdapOptions>>().Value;
            ILogger<LdapConnectionFactory>? logger = sp.GetService<ILogger<LdapConnectionFactory>>();
            return new LdapConnectionFactory(options, logger);
        });

        services.AddScoped<IActiveDirectoryService>(static sp =>
        {
            ILdapConnectionFactory factory = sp.GetRequiredService<ILdapConnectionFactory>();
            ILogger<ActiveDirectoryService>? logger = sp.GetService<ILogger<ActiveDirectoryService>>();
            return new ActiveDirectoryService(factory, logger);
        });

        return services;
    }

    /// <summary>
    /// Adds LdapHelper services using an existing LdapOptions instance.
    /// </summary>
    public static IServiceCollection AddLdapHelper(
        this IServiceCollection services,
        LdapOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(Microsoft.Extensions.Options.Options.Create(options));

        services.AddSingleton<ILdapConnectionFactory>(static sp =>
        {
            LdapOptions opt = sp.GetRequiredService<IOptions<LdapOptions>>().Value;
            ILogger<LdapConnectionFactory>? logger = sp.GetService<ILogger<LdapConnectionFactory>>();
            return new LdapConnectionFactory(opt, logger);
        });

        services.AddScoped<IActiveDirectoryService>(static sp =>
        {
            ILdapConnectionFactory factory = sp.GetRequiredService<ILdapConnectionFactory>();
            ILogger<ActiveDirectoryService>? logger = sp.GetService<ILogger<ActiveDirectoryService>>();
            return new ActiveDirectoryService(factory, logger);
        });

        return services;
    }
}
