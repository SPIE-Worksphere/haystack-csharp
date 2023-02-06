using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using ProjectHaystack.Auth;
using ProjectHaystack.io;
using ProjectHaystack.Util;

namespace ProjectHaystack.Client
{
    /// <summary>
    /// Client connection manager with a HTTP REST haystack server.
    /// </summary>
    /// <remarks>
    /// Uses HttpClient for communication, so exceptions thrown by this class can be expected.
    /// Otherwise throws HaystackException in cases the server returns a Haystack exception grid.
    /// </remarks>
    /// <seealso cref="https://project-haystack.org/doc/docHaystack/HttpApi">Project Haystack REST API docs.</seealso>
    public class HaystackClient : IHaystackClient
    {
        private static Lazy<HttpClient> _defaultClient = new Lazy<HttpClient>(() => BuildDefaultClient());
        private readonly HttpClient _client;
        private readonly IAuthenticator _authenticator;

        /// <summary>
        /// Connect using a specified HttpClient, authenticator and base uri.
        /// </summary>
        public HaystackClient(HttpClient client, IAuthenticator authenticator, Uri apiUri)
        {
            _client = client;
            _authenticator = authenticator;
            Uri = apiUri.EndWithSlash();
            AuthUri = new Uri(Uri, "/user/auth");
        }

        /// <summary>
        /// Connect using a specified authenticator and base uri, using a default HttpClient.
        /// </summary>
        public HaystackClient(IAuthenticator authentication, Uri apiUri)
            : this(_defaultClient.Value, authentication, apiUri)
        {
        }

        /// <summary>
        /// Connect using a specified base uri, using a default HttpClient and auto-detecting the authentication method.
        /// </summary>
        public HaystackClient(string username, string password, Uri apiUri)
            : this(new AutodetectAuthenticator(username, password), apiUri)
        {
        }

        /// <summary>
        /// Base URI for connection such as "http://host/api/demo/".
        /// This Uri always ends with a slash. 
        /// </summary>
        public Uri Uri { get; private set; }

        /// <summary>
        /// URI used to send authentication requests to.
        /// </summary>
        public Uri AuthUri { get; set; }

        #region Connection

        /// <summary>
        /// Open the connection, authenticating with the server.
        /// </summary>
        public virtual async Task OpenAsync()
        {
            await _authenticator.Authenticate(_client, AuthUri);
        }

        /// <summary>
        /// Close the connection, logging out the user.
        /// </summary>
        public virtual async Task CloseAsync()
        {
            using (var response = await _client.GetAsync(new Uri(Uri, "/user/logout")))
            {
                if ((int)response.StatusCode >= 400 || (int)response.StatusCode < 300)
                {
                    response.Dispose();
                    throw new HttpRequestException($"Invalid response {response.StatusCode:d}: {response.ReasonPhrase}");
                }
            }
        }

        #endregion Connection

        #region Inspection

        /// <summary>
        /// Get server summary using the "about" call.
        /// </summary>
        public async Task<HaystackDictionary> AboutAsync() => (await CallAsync("about", new HaystackGrid())).Row(0);

        /// <summary>
        /// Get list of allowed server operations using the "ops" call.
        /// </summary>
        public Task<HaystackGrid> OpsAsync() => CallAsync("ops", new HaystackGrid());

        /// <summary>
        /// Get list of MIME formats using the "formats" call.
        /// </summary>
        public Task<HaystackGrid> FormatsAsync() => CallAsync("formats", new HaystackGrid());

        #endregion Inspection

        #region Raw calls

        /// <summary>
        /// Execute a GET request and return the raw string result.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="params">Dictionary containing query parameters.</param>
        /// <param name="mimeResponse">MIME type requested for the response.</param>
        /// <returns>Raw HTTP content.</returns>
        public async Task<string> GetStringAsync(string op, Dictionary<string, string> @params, string mimeResponse = "text/zinc")
            => await GetStringAsync(op, @params, CancellationToken.None, mimeResponse);


        /// <summary>
        /// Execute a GET request and return the raw string result.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="params">Dictionary containing query parameters.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="mimeResponse">MIME type requested for the response.</param>
        /// <returns>Raw HTTP content.</returns>
        public async Task<string> GetStringAsync(string op, Dictionary<string, string> @params, CancellationToken cancellationToken, string mimeResponse = "text/zinc")
        {
            NameValueCollection queryString = HttpUtility.ParseQueryString(string.Empty);
            foreach (KeyValuePair<string, string> x in @params)
            {
                queryString[x.Key] = x.Value;
            }

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(mimeResponse));
            var response = await _client.GetAsync(new Uri(Uri, op + "?" + queryString.ToString()), cancellationToken);
            if ((int)response.StatusCode >= 300 || (int)response.StatusCode < 200)
            {
                response.Dispose();
                throw new HttpRequestException($"Invalid response {response.StatusCode:d}: {response.ReasonPhrase}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Execute a POST request and return the raw string result.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Properly formatted request string</param>
        /// <param name="mimeRequest">MIME type of the request.</param>
        /// <param name="mimeResponse">MIME type requested for the response.</param>
        /// <returns>Raw HTTP content.</returns>
        public async Task<string> PostStringAsync(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
            => await PostStringAsync(op, req, CancellationToken.None, mimeRequest, mimeResponse);

        /// <summary>
        /// Execute a POST request and return the raw string result.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Properly formatted request string</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="mimeRequest">MIME type of the request.</param>
        /// <param name="mimeResponse">MIME type requested for the response.</param>
        /// <returns>Raw HTTP content.</returns>
        public async Task<string> PostStringAsync(string op, string req, CancellationToken cancellationToken, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
        {
            var message = new HttpRequestMessage(HttpMethod.Post, new Uri(Uri, op));
            message.Headers.Accept.Clear();
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(mimeResponse));
            message.Content = new StringContent(req, Encoding.UTF8, mimeRequest);
            var response = await _client.SendAsync(message, HttpCompletionOption.ResponseContentRead, cancellationToken);
            if ((int)response.StatusCode >= 300 || (int)response.StatusCode < 200)
            {
                response.Dispose();
                throw new HttpRequestException($"Invalid response {response.StatusCode:d}: {response.ReasonPhrase}");
            }
            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Invoke an action using the "invokeAction" call.
        /// </summary>
        /// <param name="id">Target to invoke the action on.</param>
        /// <param name="action">Action to invoke.</param>
        /// <param name="args">Action arguments.</param>
        /// <returns>Action result grid.</returns>
        public Task<HaystackGrid> InvokeActionAsync(HaystackReference id, string action, HaystackDictionary args)
            => InvokeActionAsync(id, action, args, CancellationToken.None);

        /// <summary>
        /// Invoke an action using the "invokeAction" call.
        /// </summary>
        /// <param name="id">Target to invoke the action on.</param>
        /// <param name="action">Action to invoke.</param>
        /// <param name="args">Action arguments.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Action result grid.</returns>
        public Task<HaystackGrid> InvokeActionAsync(HaystackReference id, string action, HaystackDictionary args, CancellationToken cancellationToken)
        {
            var meta = new HaystackDictionary();
            meta.Add("id", id);
            meta.Add("action", new HaystackString(action));
            var req = new HaystackGrid(new[] { args }, meta);
            return CallAsync("invokeAction", req, cancellationToken);
        }


        #endregion Raw calls

        #region Reads

        /// <summary>
        /// Read all records with a given filter.
        /// </summary>
        /// <param name="filter">Record filter.</param>
        /// <param name="limit">Maximum number of results to request.</param>
        /// <returns>Grid with records.</returns>
        public Task<HaystackGrid> ReadAllAsync(string filter, int limit)
            => ReadAllAsync(filter, limit, CancellationToken.None);

        /// <summary>
        /// Read all records with a given filter.
        /// </summary>
        /// <param name="filter">Record filter.</param>
        /// <param name="limit">Maximum number of results to request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid with records.</returns>
        public Task<HaystackGrid> ReadAllAsync(string filter, int limit, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("filter");
            if (limit > 0)
            {
                req.AddColumn("limit");
                req.AddRow(new HaystackString(filter), new HaystackNumber(limit));
            }
            else
            {
                req.AddRow(new HaystackString(filter));
            }
            return CallAsync("read", req, cancellationToken);
        }

        /// <summary>
        /// Read any one record that matches a given filter.
        /// If no records apply an exception is thrown.
        /// </summary>
        /// <param name="filter">Record filter.</param>
        /// <returns>Matching record.</returns>
        public async Task<HaystackDictionary> ReadAsync(string filter)
            => await ReadAsync(filter, CancellationToken.None);

        /// <summary>
        /// Read any one record that matches a given filter.
        /// If no records apply an exception is thrown.
        /// </summary>
        /// <param name="filter">Record filter.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Matching record.</returns>
        public async Task<HaystackDictionary> ReadAsync(string filter, CancellationToken cancellationToken)
        {
            HaystackGrid grid = await ReadAllAsync(filter, 1, cancellationToken);
            if (grid.RowCount > 0)
            {
                return grid.Row(0);
            }
            throw new Exception($"Record not found for: {filter}");
        }

        /// <summary>
        /// Read all records by their given unique ID's.
        /// Throws an exception if any record was not found.
        /// </summary>
        /// <param name="ids">List of record ID's.</param>
        /// <returns>Grid with records.</returns>
        public async Task<HaystackGrid> ReadByIdsAsync(HaystackReference[] ids)
            => await ReadByIdsAsync(ids, CancellationToken.None);

        /// <summary>
        /// Read all records by their given unique ID's.
        /// Throws an exception if any record was not found.
        /// </summary>
        /// <param name="ids">List of record ID's.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid with records.</returns>
        public async Task<HaystackGrid> ReadByIdsAsync(HaystackReference[] ids, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("id");
            for (int i = 0; i < ids.Length; ++i)
            {
                req.AddRow(ids[i]);
            }
            var grid = await CallAsync("read", req, cancellationToken);
            for (int i = 0; i < grid.RowCount; ++i)
            {
                if (!grid.Row(i).ContainsKey("id"))
                {
                    throw new Exception($"Record not found for: {ids[i]}");
                }
            }
            return grid;
        }

        /// <summary>
        /// Read a single record by its unique ID.
        /// Throws an exception if the record was not found.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <returns>Matching record.</returns>
        public async Task<HaystackDictionary> ReadByIdAsync(HaystackReference id)
            => await ReadByIdAsync(id, CancellationToken.None);

        /// <summary>
        /// Read a single record by its unique ID.
        /// Throws an exception if the record was not found.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Matching record.</returns>
        public async Task<HaystackDictionary> ReadByIdAsync(HaystackReference id, CancellationToken cancellationToken)
        {
            HaystackGrid res = await ReadByIdsAsync(new HaystackReference[] { id }, cancellationToken);
            if (res.IsEmpty())
            {
                throw new Exception($"Record not found for: {id}");
            }
            HaystackDictionary rec = res.Row(0);
            if (!rec.ContainsKey("id"))
            {
                throw new Exception($"Record not found for: {id}");
            }
            return rec;
        }

        #endregion Reads

        #region Evals

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="expr">Expression.</param>
        /// <returns>Grid of results.</returns>
        public Task<HaystackGrid> EvalAsync(string expr)
            => EvalAsync(expr, CancellationToken.None);

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="expr">Expression.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of results.</returns>
        public Task<HaystackGrid> EvalAsync(string expr, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("expr")
                .AddRow(new HaystackString(expr));
            return CallAsync("eval", req, cancellationToken);
        }

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="exprs">List of expressions.</param>
        /// <returns>Grid of results per expression.</returns>
        public Task<HaystackGrid[]> EvalAllAsync(string[] exprs)
            => EvalAllAsync(exprs, CancellationToken.None);

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="exprs">List of expressions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of results per expression.</returns>
        public Task<HaystackGrid[]> EvalAllAsync(string[] exprs, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("expr");
            for (int i = 0; i < exprs.Length; ++i)
            {
                req.AddRow(new HaystackString(exprs[i]));
            }
            return EvalAllAsync(req, cancellationToken);
        }

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="req">Grid with expressions in the "expr" field.</param>
        /// <returns>Grid of results per expression.</returns>
        public async Task<HaystackGrid[]> EvalAllAsync(HaystackGrid req)
            => await EvalAllAsync(req, CancellationToken.None);

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="req">Grid with expressions in the "expr" field.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of results per expression.</returns>
        public async Task<HaystackGrid[]> EvalAllAsync(HaystackGrid req, CancellationToken cancellationToken)
        {
            var requestZinc = ZincWriter.ToZinc(req);
            var resultZinc = await PostStringAsync("evalAll", requestZinc, cancellationToken, "text/zinc");
            var result = new ZincReader(resultZinc).ReadGrids().ToArray();
            for (int i = 0; i < result.Length; ++i)
            {
                if (result[i].IsError())
                {
                    throw new HaystackException(result[i]);
                }
            }
            return result;
        }

        #endregion Evals

        #region History

        /// <summary>
        /// Read history time-series data for a given record and time range.
        /// The range has an inclusive start and an exclusive end.
        /// The range must match the timezone configured on the history record.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="range">Time range.</param>
        /// <returns>Grid of time-series data.</returns>
        public Task<HaystackGrid> HisReadAsync(HaystackReference id, HaystackDateTimeRange range)
            => HisReadAsync(id, range, CancellationToken.None);


        /// <summary>
        /// Read history time-series data for a given record and time range.
        /// The range has an inclusive start and an exclusive end.
        /// The range must match the timezone configured on the history record.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="range">Time range.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of time-series data.</returns>
        public Task<HaystackGrid> HisReadAsync(HaystackReference id, HaystackDateTimeRange range, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("id")
                .AddColumn("range")
                .AddRow(id, new HaystackString($"{ZincWriter.ToZinc(range.Start)},{ZincWriter.ToZinc(range.End)}"));
            return CallAsync("hisRead", req, cancellationToken);
        }

        /// <summary>
        /// Read history time-series data for a given point record and time range.
        /// The range has an inclusive start and an exclusive end.
        /// The range must match the timezone configured on the history record.
        /// The range will use the timezone of the record.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="range">Time range.</param>
        /// <returns>Grid of time-series data.</returns>
        public Task<HaystackGrid> HisReadAsync(HaystackReference id, string range)
            => HisReadAsync(id, range, CancellationToken.None);

        /// <summary>
        /// Read history time-series data for a given point record and time range.
        /// The range has an inclusive start and an exclusive end.
        /// The range must match the timezone configured on the history record.
        /// The range will use the timezone of the record.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="range">Time range.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of time-series data.</returns>
        public Task<HaystackGrid> HisReadAsync(HaystackReference id, string range, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("id")
                .AddColumn("range")
                .AddRow(id, new HaystackString(range));
            return CallAsync("hisRead", req, cancellationToken);
        }

        /// <summary>
        /// Write a set of history time-series data to a given point record.
        /// The record must already exist and tagged as a historized point.
        /// The timestamp timezone must exactly match the point's timezone "tz" tag.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="items">Time-series data.</param>
        /// <param name="metaData">Optional metadata to include.</param>
        public Task<HaystackGrid> HisWriteAsync(HaystackReference id, HaystackHistoryItem[] items, HaystackDictionary metaData = null)
            => HisWriteAsync(id, items, CancellationToken.None, metaData);

        /// <summary>
        /// Write a set of history time-series data to a given point record.
        /// The record must already exist and tagged as a historized point.
        /// The timestamp timezone must exactly match the point's timezone "tz" tag.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="items">Time-series data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="metaData">Optional metadata to include.</param>
        public Task<HaystackGrid> HisWriteAsync(HaystackReference id, HaystackHistoryItem[] items, CancellationToken cancellationToken, HaystackDictionary metaData = null)
        {
            var meta = metaData ?? new HaystackDictionary();
            meta.Add("id", id);
            HaystackGrid req = new HaystackGrid(items, meta);
            return CallAsync("hisWrite", req, cancellationToken);
        }

        #endregion History

        #region Points

        /// <summary>
        /// Write to a given level of a writable point, and return the current status
        /// of a writable point's priority array <see cref="pointWriteArray"/>.
        /// </summary>
        /// <param name="id">Reference of a writable point.</param>
        /// <param name="level">Number for level to write [1-17].</param>
        /// <param name="who">Username performing the write, defaults to user dis.</param>
        /// <param name="val">Value to write or null to auto the level.</param>
        /// <param name="dur">Number with duration unit if setting level 8.</param>
        /// <returns>Result grid.</returns>
        public Task<HaystackGrid> PointWriteAsync(HaystackReference id, int level, string who, HaystackValue val, HaystackNumber dur)
            => PointWriteAsync(id, level, who, val, dur, CancellationToken.None);

        /// <summary>
        /// Write to a given level of a writable point, and return the current status
        /// of a writable point's priority array <see cref="pointWriteArray"/>.
        /// </summary>
        /// <param name="id">Reference of a writable point.</param>
        /// <param name="level">Number for level to write [1-17].</param>
        /// <param name="who">Username performing the write, defaults to user dis.</param>
        /// <param name="val">Value to write or null to auto the level.</param>
        /// <param name="dur">Number with duration unit if setting level 8.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Result grid.</returns>
        public Task<HaystackGrid> PointWriteAsync(HaystackReference id, int level, string who, HaystackValue val, HaystackNumber dur, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("id")
                .AddColumn("level")
                .AddColumn("who")
                .AddColumn("val")
                .AddColumn("duration")
                .AddRow(id, new HaystackNumber(level), new HaystackString(who), val, dur);
            return CallAsync("pointWrite", req, cancellationToken);
        }

        /// <summary>
        /// Return the current status of a point's priority array.
        /// The resulting grid has the following columns:
        /// - level: number [1-17] (17 is default)
        /// - levelDis: Human description of level
        /// - val: current value at level or null
        /// - who: who last controlled the value at this level
        /// </summary>
        /// <param name="id">Reference of a writable point.</param>
        /// <returns>Result grid.</returns>
        public Task<HaystackGrid> PointWriteArrayAsync(HaystackReference id)
            => PointWriteArrayAsync(id, CancellationToken.None);

        /// <summary>
        /// Return the current status of a point's priority array.
        /// The resulting grid has the following columns:
        /// - level: number [1-17] (17 is default)
        /// - levelDis: Human description of level
        /// - val: current value at level or null
        /// - who: who last controlled the value at this level
        /// </summary>
        /// <param name="id">Reference of a writable point.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Result grid.</returns>
        public Task<HaystackGrid> PointWriteArrayAsync(HaystackReference id, CancellationToken cancellationToken)
        {
            var req = new HaystackGrid()
                .AddColumn("id")
                .AddRow(id);
            return CallAsync("pointWrite", req, cancellationToken);
        }


        #endregion Points

        /// <summary>
        /// Call the given operation and throw server-side exceptions.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Request content.</param>
        /// <returns>Grid of results.</returns>
        public async Task<HaystackGrid> CallAsync(string op, HaystackGrid req = null)
            => await CallAsync(op, req, CancellationToken.None);

        /// <summary>
        /// Call the given operation and throw server-side exceptions.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Request content.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of results.</returns>
        public async Task<HaystackGrid> CallAsync(string op, HaystackGrid req, CancellationToken cancellationToken)
        {
            HaystackGrid res;
            if (req == null || req.IsEmpty())
            {
                 res = await GetAsync(op, new Dictionary<string, string>(), cancellationToken);
            }
            else
            {
                 res = await PostGridAsync(op, req, cancellationToken);
            }

            if (res.IsError())
            {
                throw new HaystackException(res);
            }
            return res;
        }

        /// <summary>
        /// Execute a POST request and parse the raw string result into a grid.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Properly formatted request string</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of results.</returns>
        private async Task<HaystackGrid> PostGridAsync(string op, HaystackGrid req, CancellationToken cancellationToken)
        {
            string reqStr = ZincWriter.ToZinc(req);
            string resStr = await PostStringAsync(op, reqStr, cancellationToken, "text/zinc");
            return new ZincReader(resStr).ReadValue<HaystackGrid>();
        }


        /// <summary>
        /// Execute a GET request and parse the raw string result into a grid.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="params">Parameters for the GET request.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Grid of results.</returns>
        private async Task<HaystackGrid> GetAsync(string op, Dictionary<string, string> @params, CancellationToken cancellationToken)
        {
            string resStr = await GetStringAsync(op, @params, cancellationToken);
            return new ZincReader(resStr).ReadValue<HaystackGrid>();
        }

        /// <summary>
        /// Build the default HttpClient.
        /// </summary>
        /// <returns>HttpClient.</returns>
        private static HttpClient BuildDefaultClient()
        {
            var handler = new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false };
            var defaultClient = new HttpClient(handler);
            defaultClient.DefaultRequestHeaders.Add("User-Agent", "HaystackC#");
            return defaultClient;
        }
    }
}