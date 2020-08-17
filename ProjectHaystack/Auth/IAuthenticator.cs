using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
    /// <summary>
    /// Defines an authentication mechanism using a <c>HttpClient</c>.
    /// </summary>
    public interface IAuthenticator
    {
        Task Authenticate(HttpClient client, Uri authUrl);
    }
}