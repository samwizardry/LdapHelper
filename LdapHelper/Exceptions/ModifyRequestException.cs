using System.DirectoryServices.Protocols;

namespace LdapHelper.Exceptions;

public class ModifyRequestException : Exception
{
    public ResultCode ResultCode { get; }

    public ModifyResponse Response { get; }

    public ModifyRequestException(ResultCode resultCode, string message, ModifyResponse response)
        : base(message)
    {
        ResultCode = resultCode;
        Response = response;
    }
}
