using System.DirectoryServices.Protocols;

namespace LdapHelper.Exceptions;

public class LdapOperationException : Exception
{
    public ResultCode ResultCode { get; }

    public DirectoryResponse? Response { get; }

    public LdapOperationException(ResultCode resultCode, string? message, DirectoryResponse? response = null)
        : base(string.IsNullOrWhiteSpace(message) ? $"LDAP operation failed with result code {resultCode} ({(int)resultCode})." : message)
    {
        ResultCode = resultCode;
        Response = response;
    }

    public LdapOperationException(ResultCode resultCode, string? message, Exception? innerException, DirectoryResponse? response = null)
        : base(string.IsNullOrWhiteSpace(message) ? $"LDAP operation failed with result code {resultCode} ({(int)resultCode})." : message, innerException)
    {
        ResultCode = resultCode;
        Response = response;
    }
}
