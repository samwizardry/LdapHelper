using System.DirectoryServices.Protocols;

namespace LdapHelper.Exceptions;

public class ModifyRequestException : LdapOperationException
{
    public new ModifyResponse? Response => (ModifyResponse?)base.Response;

    public ModifyRequestException(ResultCode resultCode, string? message, ModifyResponse? response = null)
        : base(resultCode, string.IsNullOrWhiteSpace(message) ? $"LDAP modification failed with result code {resultCode} ({(int)resultCode})." : message, response)
    {
    }

    public ModifyRequestException(ResultCode resultCode, string? message, Exception? innerException, ModifyResponse? response = null)
        : base(resultCode, string.IsNullOrWhiteSpace(message) ? $"LDAP modification failed with result code {resultCode} ({(int)resultCode})." : message, innerException, response)
    {
    }
}
