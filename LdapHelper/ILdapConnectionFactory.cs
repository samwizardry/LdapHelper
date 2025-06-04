using System.DirectoryServices.Protocols;

namespace LdapHelper;

public interface ILdapConnectionFactory
{
    LdapConnection CreateConnection();

    LdapConnection CreateConnection(string username, string password);
}
