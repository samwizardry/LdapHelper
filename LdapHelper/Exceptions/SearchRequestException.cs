using System.DirectoryServices.Protocols;

namespace LdapHelper.Exceptions;

public class SearchRequestException : LdapOperationException
{
    public new SearchResponse? Response => (SearchResponse?)base.Response;

    public SearchRequestException(ResultCode resultCode, string? message, SearchResponse? response = null)
        : base(resultCode, string.IsNullOrWhiteSpace(message) ? $"LDAP search failed with result code {resultCode} ({(int)resultCode})." : message, response)
    {
    }

    public SearchRequestException(ResultCode resultCode, string? message, Exception? innerException, SearchResponse? response = null)
        : base(resultCode, string.IsNullOrWhiteSpace(message) ? $"LDAP search failed with result code {resultCode} ({(int)resultCode})." : message, innerException, response)
    {
    }
}
