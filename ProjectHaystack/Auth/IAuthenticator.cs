using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ProjectHaystack.Auth
{
    public interface IAuthenticator
    {
        Task Authenticate(HttpClient client, Uri authUrl);
    }
}