using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHaystack.Client
{
    public interface IHAsyncClient : IHClient, IHProj
    {
        Task<HGrid[]> EvalAllAsync(HGrid req, bool bChecked);
        Task<string> GetStringAsync(string op, Dictionary<string, string> @params, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc");
        Task<string> PostStringAsync(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc");
        Task<HGrid> readAllAsync(string filter, int limit);
        Task<HDict> readAsync(string filter, bool bChecked);
    }
}