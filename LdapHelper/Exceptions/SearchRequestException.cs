using System.DirectoryServices.Protocols;

namespace LdapHelper.Exceptions;

public class SearchRequestException : Exception
{
    public ResultCode ResultCode { get; }

    public SearchResponse Response { get; }

    public SearchRequestException(ResultCode resultCode, string message, SearchResponse response)
        : base (message)
    {
        ResultCode = resultCode;
        Response = response;
    }
}
