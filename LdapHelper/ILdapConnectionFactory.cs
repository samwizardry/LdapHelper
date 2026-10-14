using System.DirectoryServices.Protocols;

namespace LdapHelper;

public interface ILdapConnectionFactory
{
    LdapOptions Options { get; }

    LdapConnection CreateConnection();

    LdapConnection CreateConnection(string username, string password);
}
