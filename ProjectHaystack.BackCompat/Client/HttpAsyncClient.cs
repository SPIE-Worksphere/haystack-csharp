using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using ProjectHaystack.Auth;
using ProjectHaystack.io;
using M = ProjectHaystack.HaystackValueMapper;

namespace ProjectHaystack.Client
{

    [Obsolete("Use HaystackClient", false)]
    public class HttpAsyncClient : IHClient, IHProj, IHAsyncClient
    {
        private readonly HaystackClient _sourceClient;

        public HttpAsyncClient(HttpClient client, IAuthenticator authenticator, Uri apiUri)
        {
            _sourceClient = new HaystackClient(client, authenticator, apiUri);
        }

        public HttpAsyncClient(IAuthenticator authenticator, Uri apiUri)
        {
            _sourceClient = new HaystackClient(authenticator, apiUri);
        }

        public HttpAsyncClient(string username, string password, Uri apiUri)
        {
            _sourceClient = new HaystackClient(username, password, apiUri);
        }

        public Uri Uri => _sourceClient.Uri;
        public Uri AuthUri { get => _sourceClient.AuthUri; set => _sourceClient.AuthUri = value; }
        public virtual Task OpenAsync() => _sourceClient.OpenAsync();
        public virtual Task CloseAsync() => _sourceClient.CloseAsync();
        public HDict about() => M.Map(_sourceClient.AboutAsync().Result);
        public HGrid ops() => M.Map(_sourceClient.OpsAsync().Result);
        public HGrid formats() => M.Map(_sourceClient.FormatsAsync().Result);
        public Task<string> GetStringAsync(string op, Dictionary<string, string> @params, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
            => _sourceClient.GetStringAsync(op, @params, mimeResponse);
        public Task<string> PostStringAsync(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
            => _sourceClient.PostStringAsync(op, req, mimeRequest, mimeResponse);
        public HDict readById(HRef id, bool bChecked) => M.Checked(() => M.Map(_sourceClient.ReadByIdAsync(M.Map(id)).Result), bChecked);
        public HDict readById(HRef id) => M.Map(_sourceClient.ReadByIdAsync(M.Map(id)).Result);
        public HGrid readByIds(HRef[] ids, bool bChecked) => M.Checked(() => M.Map(_sourceClient.ReadByIdsAsync(M.Map(ids)).Result), bChecked);
        public HGrid readByIds(HRef[] ids) => M.Map(_sourceClient.ReadByIdsAsync(M.Map(ids)).Result);
        public Task<HDict> readAsync(string filter, bool bChecked) => M.Checked(async () => M.Map(await _sourceClient.ReadAsync(filter)), bChecked);
        public HDict read(string filter, bool bChecked) => M.Checked(() => M.Map(_sourceClient.ReadAsync(filter).Result), bChecked);
        public async Task<HGrid> readAllAsync(string filter, int limit) => M.Map(await _sourceClient.ReadAllAsync(filter, limit));
        public HGrid readAll(string filter, int limit) => M.Map(_sourceClient.ReadAllAsync(filter, limit).Result);
        public HGrid eval(string expr) => M.Map(_sourceClient.EvalAsync(expr).Result);
        public HGrid[] evalAll(string[] exprs) => M.Map(_sourceClient.EvalAllAsync(exprs).Result);
        public HGrid[] evalAll(string[] exprs, bool bChecked) => M.Checked(() => M.Map(_sourceClient.EvalAllAsync(exprs).Result), bChecked);
        public Task<HGrid[]> EvalAllAsync(HGrid req, bool bChecked) => M.Checked(async () => M.Map(await _sourceClient.EvalAllAsync(M.Map(req))), bChecked);
        public HGrid[] evalAll(HGrid req, bool bChecked) => M.Checked(() => M.Map(_sourceClient.EvalAllAsync(M.Map(req)).Result), bChecked);
        public HGrid pointWrite(HRef id, int level, string who, HVal val, HNum dur) => M.Map(_sourceClient.PointWriteAsync(M.Map(id), level, who, M.Map(val), M.Map(dur)).Result);
        public HGrid pointWriteArray(HRef id) => M.Map(_sourceClient.PointWriteArrayAsync(M.Map(id)).Result);
        public HGrid hisRead(HRef id, object range)
        {
            if (range is HDateTimeRange dtRange)
            {
                return M.Map(_sourceClient.HisReadAsync(M.Map(id), M.Map(dtRange)).Result);
            }
            return M.Map(_sourceClient.HisReadAsync(M.Map(id), range.ToString()).Result);
        }
        public void hisWrite(HRef id, HHisItem[] items) => _sourceClient.HisWriteAsync(M.Map(id), M.Map(items));
        public async Task<HGrid> hisWriteAsync(HRef id, HHisItem[] items) => M.Map(await _sourceClient.HisWriteAsync(M.Map(id), M.Map(items)));
        public async Task<HGrid> HisWriteAsync(HRef id, HHisItem[] items, HDict metaData = null) => M.Map(await _sourceClient.HisWriteAsync(M.Map(id), M.Map(items), M.Map(metaData)));
        public async Task<HGrid> HisWriteNoWarnAsync(HRef id, HHisItem[] items) => M.Map(await _sourceClient.HisWriteNoWarnAsync(M.Map(id), M.Map(items)));
        public HGrid invokeAction(HRef id, string action, HDict args) => M.Map(_sourceClient.InvokeActionAsync(M.Map(id), action, M.Map(args)).Result);
        public HGrid invokeAction(HRef id, string action, HDict args, string mimetype) => M.Map(_sourceClient.InvokeActionAsync(M.Map(id), action, M.Map(args)).Result);
        public async Task<HGrid> CallAsync(string op, HGrid req, string mimeType) => M.Map(ZincReader.ReadValue<HaystackGrid>(await _sourceClient.PostStringAsync(op, ZincWriter.ToZinc(M.Map(req)))));
        public HGrid call(string op, HGrid req, string mimeType) => M.Map(ZincReader.ReadValue<HaystackGrid>(_sourceClient.PostStringAsync(op, ZincWriter.ToZinc(M.Map(req))).Result));
        public HGrid call(string op, HGrid req) => M.Map(ZincReader.ReadValue<HaystackGrid>(_sourceClient.PostStringAsync(op, ZincWriter.ToZinc(M.Map(req))).Result));
    }
}