//
// Copyright (c) 2017, SkyFoundry LLC
// (Ian Davies) Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   26 Jun 2017    Hank Weber  Creation
//   4 April 2018   Ian Davies  Extend to implement more haystack
//                              based on java tool kit (see Haystack.org)
//   8 August 2018  Ian Davies  Tabbing and clean up to align with normal VS practises
//                              fields renamed and initialised in ctor - also mimetype 
//                              changed to default to zinc over plain text as we have 
//                              both a ZincReader and ZincWriter implemented.
//

using System;
using System.Web;
using System.Collections;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using ProjectHaystack.io;


namespace ProjectHaystack.Client
{
    using AuthClientContext = ProjectHaystack.Auth.AuthClientContext;
    using AuthMsg = ProjectHaystack.Auth.AuthMsg;
    using Base64 = ProjectHaystack.Util.Base64;
    using Pbkdf2 = ProjectHaystack.Util.Pbkdf2;
    using Scramscheme = ProjectHaystack.Auth.ScramScheme;

    // NOTE: I don't like this OO design - to me this acts more like a struct
    //  to me the way this works with HClient is it breaks encapsulation (granted they had as an internal
    //  class which I changed)  - might in the future do a OO reorg here.
    public class HClientWatch : HWatch
    {
        private HClient m_client;
        private string m_dis;
        private HNum m_desiredLease;
        private string m_strID;
        private HNum m_lease;
        private bool m_bClosed;

        public HClientWatch(HClient c, string d, HNum l)
        {
            m_client = c;
            m_dis = d;
            m_desiredLease = l;
        }
        public string ID
        {
            get { return m_strID; } // Technically replicating the inherited function
            set { m_strID = value; }
        }
        public HNum Lease
        {
            get { return m_lease; } // Technically replicating the inherited function
            set { m_lease = value; }
        }
        public bool Closed
        {
            get { return m_bClosed; }
            set { m_bClosed = value; }
        }
        public override string id()
        {
            return m_strID;
        }
        public override HNum lease()
        {
            return m_lease;
        }
        public override string dis()
        {
            return m_dis;
        }
        public override HGrid sub(HRef[] ids, bool bChecked)
        {
            return m_client.watchSub(this, ids, bChecked);
        }
        public override void unsub(HRef[] ids)
        {
            m_client.watchUnsub(this, ids);
        }
        public override HGrid pollChanges()
        {
            return m_client.watchPoll(this, false);
        }
        public override HGrid pollRefresh()
        {
            return m_client.watchPoll(this, true);
        }
        public override void close()
        {
            m_client.watchClose(this, true);
        }
        public override bool isOpen()
        {
            return !m_bClosed;
        }
    }
    /// <summary>
    /// HClient manages a logical connection to a HTTP REST haystack server.
    /// </summary>
    /// <seealso> cref= <a href='http://project-haystack.org/doc/Rest'>Project Haystack</a> </seealso>
    public class HClient : HProj
    {
        //////////////////////////////////////////////////////////////////////////
        // Fields
        //////////////////////////////////////////////////////////////////////////

        private AuthClientContext m_auth;
        private Hashtable m_watches_Renamed;

        //////////////////////////////////////////////////////////////////////////
        // State
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Base URI for connection such as "http://host/api/demo/".
        ///    This string always ends with slash. 
        /// </summary>
        private string m_strUri;

        /// <summary>
        /// Timeout in milliseconds for opening the HTTP socket </summary>
        private int m_iConnectTimeout;

        /// <summary>
        /// Timeout in milliseconds for reading from the HTTP socket </summary>
        public int m_iReadTimeout;

        //////////////////////////////////////////////////////////////////////////
        // Access
        //////////////////////////////////////////////////////////////////////////
        public string uri { get { return m_strUri; } }
        public int ConnectTimeout { get { return m_iConnectTimeout; } }

        //////////////////////////////////////////////////////////////////////////
        // Construction
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Convenience for construction and call to Open().
        /// </summary>
        public static HClient Open(string uri, string user, string pass)
        {
            return (new HClient(uri, user, pass)).Open();
        }

        /// <summary>
        /// Convenience for constructing client with custom timeouts and call to Open()
        /// </summary>
        public static HClient Open(string uri, string user, string pass, int connectTimeout, int readTimeout)
        {
            return (new HClient(uri, user, pass)).SetTimeouts(connectTimeout, readTimeout).Open();
        }

        /// <summary>
        /// Constructor with URI to server's API and authentication credentials.
        /// </summary>
        public HClient(string uri, string user, string pass)
        {
            m_iConnectTimeout = 60 * 1000;
            m_iReadTimeout = 60 * 1000;
            m_watches_Renamed = new Hashtable();
            // check uri
            if (!uri.StartsWith("http://", StringComparison.Ordinal) && !uri.StartsWith("https://", StringComparison.Ordinal))
            {
                throw new System.ArgumentException("Invalid uri format: " + uri);
            }
            if (!uri.EndsWith("/", StringComparison.Ordinal))
            {
                uri = uri + "/";
            }

            // sanity check arguments
            if (user.Length == 0)
            {
                throw new System.ArgumentException("user cannot be empty string");
            }

            m_strUri = uri;
            m_auth = new AuthClientContext(uri + "about", user, pass);
        }


        /// <summary>
        /// Set the connect timeout and return this </summary>
        public virtual HClient SetConnectTimeout(int timeout)
        {
            if (timeout < 0)
            {
                throw new System.ArgumentException("Invalid timeout: " + timeout);
            }
            m_iConnectTimeout = timeout;
            return this;
        }


        /// <summary>
        /// Set the read timeout and return this </summary>
        public virtual HClient SetReadTimeout(int timeout)
        {
          if (timeout < 0)
          {
            throw new System.ArgumentException("Invalid timeout: " + timeout);
          }
          m_iReadTimeout = timeout;
          return this;
        }

        /// <summary>
        /// Set the connect and read timeouts and return this </summary>
        public virtual HClient SetTimeouts(int connectTimeout, int readTimeout)
        {
            return SetConnectTimeout(connectTimeout).SetReadTimeout(readTimeout);
        }

        //////////////////////////////////////////////////////////////////////////
        // Operations
        //////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Authenticate the client and return this.
        /// </summary>
        public virtual HClient Open()
        {
          m_auth.connectTimeout = m_iConnectTimeout;
          m_auth.readTimeout = m_iReadTimeout;
          m_auth.Open();
          return this;
        }

        #region AddedFromJavaToolkitOperations
        // Added from Java Toolkit
        // Call "about" to query summary info.
        public override HDict about()
        {
            return call("about", HGrid.InstanceEmpty).row(0);
        }

        /**
         * Call "ops" to query which operations are supported by server.
         */
        public HGrid ops()
        {
            return call("ops", HGrid.InstanceEmpty);
        }

        /**
         * Call "formats" to query which MIME formats are available.
         */
        public HGrid formats()
        {
            return call("formats", HGrid.InstanceEmpty);
        }
        #endregion //AddedFromJavaToolkitOperations

        /// <summary>
        /// Gets the raw string from request passed in
        /// </summary>
        /// <param name="op">Given operation</param>
        /// <param name="params">Dictionary containing search parameters</param>
        /// <param name="mimeRequest">Mime type for ContentType header</param>
        /// <param name="mimeResponse">Mime type for Accept header</param>
        /// <returns>Raw string of the result</returns>
        public string GetString(string op, Dictionary<string, string> @params, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
        {
            var builder = new UriBuilder(uri + op);
            NameValueCollection queryString = HttpUtility.ParseQueryString(String.Empty);
            foreach (KeyValuePair<string, string> x in @params)
            {
                queryString[x.Key] = x.Value;
            }
            builder.Query = queryString.ToString();
            var c = OpenHttpConnection(builder.Uri, "GET");
            c = m_auth.Prepare(c);
            c.ContentType = mimeRequest ?? "text/plain";
            c.Accept = mimeResponse ?? "text/plain";
            using (var resp = (HttpWebResponse)c.GetResponse())
            {
                var sr = new StreamReader(resp.GetResponseStream());
                return sr.ReadToEnd();
            }
   
        }

        /// <summary>
        /// Make a call with the given operation and post to the uri. Response is returned as a string.
        /// </summary>
        /// <param name="op">Given operation</param>
        /// <param name="req">Properly formatted request string</param>
        /// <param name="mimeRequest">Mime type for ContentType header</param>
        /// <param name="mimeResponse">Mime type for Accept header</param>
        /// <returns>Raw string of the result</returns>
        public string PostString(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
        {
            var builder = new UriBuilder(this.uri + op);
            var c = OpenHttpConnection(builder.Uri, "POST");
            c = m_auth.Prepare(c);
            c.Method = "POST";
            c.ContentType = mimeRequest ?? "text/plain";
            c.Accept = mimeResponse ?? "text/plain";
            // c.ContentType = mimeRequest == null ? "text/plain; charset=utf-8" : mimeRequest + "; charset=utf-8";
            // c.Accept = mimeResponse == null ? "text/plain; charset=utf-8" : mimeResponse + "; charset=utf-8";
            byte[] data = Encoding.ASCII.GetBytes(req);
            c.ContentLength = data.Length;
            Stream stream = c.GetRequestStream();
            stream.Write(data, 0, data.Length);
            stream.Close();
            WebResponse webResp = c.GetResponse();
            stream = webResp.GetResponseStream();
            StreamReader sr = new StreamReader(stream);

            return sr.ReadToEnd();
        }

        #region AddedFromJavaRegionReads
        //////////////////////////////////////////////////////////////////////////
        // Reads
        //////////////////////////////////////////////////////////////////////////

        protected override HDict onReadById(HRef id)
        {
            HGrid res = readByIds(new HRef[] { id }, false);
            if (res.isEmpty())
                return null;
            HDict rec = res.row(0);
            if (rec.missing("id"))
                return null;
            return rec;
        }

        protected override HGrid onReadByIds(HRef[] ids)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("id");
            for (int i = 0; i < ids.Length; ++i)
                b.addRow(new HVal[] { ids[i] });
            HGrid req = b.toGrid();
            return call("read", req);
        }

        protected override HGrid onReadAll(String filter, int limit)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("filter");
            b.addCol("limit");
            b.addRow(new HVal[] { HStr.make(filter), HNum.make(limit) });
            HGrid req = b.toGrid();
            return call("read", req);
        }
        #endregion // AddedFromJavaRegionReads

        #region AddedFromJavaRegionEvals
        //////////////////////////////////////////////////////////////////////////
        // Evals
        //////////////////////////////////////////////////////////////////////////

        /**
         * Call "eval" operation to evaluate a vendor specific
         * expression on the server:
         *   - SkySpark: any Axon expression
         *
         * Raise CallErrException if the server raises an exception.
         */
        public HGrid eval(string expr)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("expr");
            b.addRow(new HVal[] { HStr.make(expr) });
            HGrid req = b.toGrid();
            return call("eval", req);
        }

        /**
         * Convenience for "evalAll(HGrid, true)".
         */
        public HGrid[] evalAll(string[] exprs)
        {
            return evalAll(exprs, true);
        }

        /**
         * Convenience for "evalAll(HGrid, checked)".
         */
        public HGrid[] evalAll(string[] exprs, bool bChecked)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("expr");
            for (int i = 0; i < exprs.Length; ++i)
                b.addRow(new HVal[] { HStr.make(exprs[i]) });
            return evalAll(b.toGrid(), bChecked);
        }

        /**
         * Call "evalAll" operation to evaluate a batch of vendor specific
         * expressions on the server. See "eval" method for list of vendor
         * expression formats.  The request grid must specify an "expr" column.
         * A separate grid is returned for each row in the request.  If checked
         * is false, then this call does *not* automatically check for error
         * grids.  Client code must individual check each grid for partial
         * failures using "Grid.isErr".  If checked is true and one of the
         * requests failed, then raise CallErrException for first failure.
         */
        public HGrid[] evalAll(HGrid req, bool bChecked)
        {
            string reqStr = HZincWriter.gridToString(req);
            string resStr = postString(uri + "evalAll", reqStr);
            HGrid[] res = new HZincReader(resStr).readGrids();
            if (bChecked)
            {
                for (int i = 0; i < res.Length; ++i)
                    if (res[i].isErr()) 
                        throw new CallErrException(res[i]);
            }
            return res;
        }
        #endregion // AddedFromJavaRegionEvals

        #region AddedFromJavaRegionWatches
        //////////////////////////////////////////////////////////////////////////
        // Watches
        //////////////////////////////////////////////////////////////////////////

        /**
         * Create a new watch with an empty subscriber list.  The dis
         * string is a debug string to keep track of who created the watch.
         */
        public override HWatch watchOpen(string dis, HNum lease)
        {
            return new HClientWatch(this, dis, lease);
        }

        /**
         * List the open watches associated with this HClient.
         * This list does *not* contain a watch until it has been successfully
         * subscribed and assigned an identifier by the server.
         */
        public override HWatch[] watches()
        {
            HWatch[] watchArray = new HWatch[m_watches_Renamed.Count];
            m_watches_Renamed.Values.CopyTo(watchArray, 0);
            return (watchArray);
        }

        /**
         * Lookup a watch by its unique identifier associated with this HClient.
         * If not found return null or raise UnknownWatchException based on
         * checked flag.
         */
        public override HWatch watch(string id, bool bChecked)
        {
            HWatch w = (HWatch)m_watches_Renamed[id];
            if (w != null) return w;
            if (bChecked)
                throw new Exception("watch not found for: " + id);
            return null;
        }

        public HGrid watchSub(HClientWatch w, HRef[] ids, bool bChecked)
        {
            if (ids.Length == 0)
                throw new ArgumentException("ids are empty", "ids");
            if (w.Closed)
                throw new InvalidOperationException("watch is closed");

            // grid meta
            HGridBuilder b = new HGridBuilder();
            if (w.ID != null) b.Meta.add("watchId", w.ID);
            if (w.Lease != null) b.Meta.add("lease", w.Lease);
            b.Meta.add("watchDis", w.dis());

            // grid rows
            b.addCol("id");
            for (int i = 0; i < ids.Length; ++i)
                b.addRow(new HVal[] { ids[i] });

            // make request
            HGrid res;
            try
            {
                HGrid req = b.toGrid();
                res = call("watchSub", req);
            }
            catch (CallErrException e)
            {
                // any server side error is considered close
                watchClose(w, false);
                throw e;
            }

            // make sure watch is stored with its watch id
            if (w.ID == null)
            {
                w.ID = res.meta.getStr("watchId");
                w.Lease = (HNum)res.meta.get("lease");
                m_watches_Renamed.Add(w.ID, w);
            }

            // if checked, then check it
            if (bChecked)
            {
                if (res.numRows != ids.Length && ids.Length > 0)
                    throw new Exception("unknwon record " + ids[0]);
                for (int i = 0; i < res.numRows; ++i)
                    if (res.row(i).missing("id"))
                        throw new Exception("unknwon record " + ids[i]);
            }
            return res;
        }

        public void watchUnsub(HClientWatch w, HRef[] ids)
        {
            if (ids.Length == 0) throw new ArgumentException("ids are empty", "ids");
            if (w.ID == null) throw new InvalidOperationException("nothing subscribed yet");
            if (w.Closed) throw new InvalidOperationException("watch is closed");

            // grid meta
            HGridBuilder b = new HGridBuilder();
            b.Meta.add("watchId", w.ID);

            // grid rows
            b.addCol("id");
            for (int i = 0; i < ids.Length; ++i)
                b.addRow(new HVal[] { ids[i] });

            // make request
            HGrid req = b.toGrid();
            call("watchUnsub", req);
        }

        public HGrid watchPoll(HClientWatch w, bool bRefresh)
        {
            if (w.ID == null) throw new InvalidOperationException("nothing subscribed yet");
            if (w.Closed) throw new InvalidOperationException("watch is closed");

            // grid meta
            HGridBuilder b = new HGridBuilder();
            b.Meta.add("watchId", w.ID);
            if (bRefresh)
                b.Meta.add("refresh");
            b.addCol("empty");

            // make request
            HGrid req = b.toGrid();
            try
            {
                return call("watchPoll", req);
            }
            catch (CallErrException e)
            {
                // any server side error is considered close
                watchClose(w, false);
                throw e;
            }
        }

        public void watchClose(HClientWatch w, bool bSend)
        {
            // mark flag on watch itself, short circuit if already closed
            if (w.Closed) return;
            w.Closed = true;

            // remove it from my lookup table
            if (w.ID != null) m_watches_Renamed.Remove(w.ID);

            // optionally send close message to server
            if (bSend)
            {
                try
                {
                    HGridBuilder b = new HGridBuilder();
                    b.Meta.add("watchId", w.ID).add("close");
                    b.addCol("id");
                    call("watchUnsub", b.toGrid());
                }
                catch (Exception /*e*/)
                {
                    // YET TO DO - we aren't doing anything here, why?
                }
            }
        }

        #endregion // AddedFromJavaRegionWatches
        #region AddedFromJavaRegionPointWrites
        //////////////////////////////////////////////////////////////////////////
        // PointWrite
        //////////////////////////////////////////////////////////////////////////

        /**
          * Write to a given level of a writable point, and return the current status
          * of a writable point's priority array (see pointWriteArray()).
          *
          * @param id Ref identifier of writable point
          * @param level Number from 1-17 for level to write
          * @param val value to write or null to auto the level
          * @param who optional username performing the write, otherwise user dis is used
          * @param dur Number with duration unit if setting level 8
          */
        public HGrid pointWrite(HRef id, int level, string who,
                HVal val, HNum dur)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("id");
            b.addCol("level");
            b.addCol("who");
            b.addCol("val");
            b.addCol("duration");

            b.addRow(new HVal[] {
                id,
                HNum.make(level),
                HStr.make(who),
                val,
                dur });

            HGrid req = b.toGrid();
            HGrid res = call("pointWrite", req);
            return res;
        }

        /**
          * Return the current status
          * of a point's priority array.
          * The result is returned grid with following columns:
          * <ul>
          *   <li>level: number from 1 - 17 (17 is default)
          *   <li>levelDis: human description of level
          *   <li>val: current value at level or null
          *   <li>who: who last controlled the value at this level
          * </ul>
          */
        public HGrid pointWriteArray(HRef id)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("id");
            b.addRow(new HVal[] { id });

            HGrid req = b.toGrid();
            HGrid res = call("pointWrite", req);
            return res;
        }

        #endregion // AddedFromJavaRegionPointWrites
        #region AddedFromJavaRegionHistory
        //////////////////////////////////////////////////////////////////////////
        // History
        //////////////////////////////////////////////////////////////////////////

        /**
         * Read history time-series data for given record and time range. The
         * items returned are exclusive of start time and inclusive of end time.
         * Raise exception if id does not map to a record with the required tags
         * "his" or "tz".  The range may be either a String or a HDateTimeRange.
         * If HTimeDateRange is passed then must match the timezone configured on
         * the history record.  Otherwise if a String is passed, it is resolved
         * relative to the history record's timezone.
         */
        public override HGrid hisRead(HRef id, object range)
        {
            HGridBuilder b = new HGridBuilder();
            b.addCol("id");
            b.addCol("range");
            b.addRow(new HVal[] { id, HStr.make(range.ToString()) });
            HGrid req = b.toGrid();
            HGrid res = call("hisRead", req);
            return res;
        }

        /**
         * Write a set of history time-series data to the given point record.
         * The record must already be defined and must be properly tagged as
         * a historized point.  The timestamp timezone must exactly match the
         * point's configured "tz" tag.  If duplicate or out-of-order items are
         * inserted then they must be gracefully merged.
         */
        public override void hisWrite(HRef id, HHisItem[] items)
        {
            HDict meta = new HDictBuilder().add("id", id).toDict();
            HGrid req = HGridBuilder.hisItemsToGrid(meta, items);
            call("hisWrite", req);
        }
        #endregion // AddedFromJavaRegionHistory

        #region AddedFromJavaActionsAndCall
        //////////////////////////////////////////////////////////////////////////
        // Actions
        //////////////////////////////////////////////////////////////////////////

        /**
         * Invoke a remote action using the "invokeAction" REST operation.
         */
        public HGrid invokeAction(HRef id, string action, HDict args)
        {
            HDict meta = new HDictBuilder().add("id", id).add("action", action).toDict();
            HGrid req = HGridBuilder.dictsToGrid(meta, new HDict[] { args });
            return call("invokeAction", req);
        }

        // Depart from Java - This has a method where mimetype selection can be specified - NOTE: This toolkit
        //    does not yet support a JsonReader
        public HGrid invokeAction(HRef id, string action, HDict args, string mimetype)
        {
            HDict meta = new HDictBuilder().add("id", id).add("action", action).toDict();
            HGrid req = HGridBuilder.dictsToGrid(meta, new HDict[] { args });
            return call("invokeAction", req, mimetype);
        }

        //////////////////////////////////////////////////////////////////////////
        // Call
        //////////////////////////////////////////////////////////////////////////

        /**
         * Make a call to the given operation.  The request grid is posted
         * to the URI "this.uri+op" and the response is parsed as a grid.
         * Raise CallNetworkException if there is a communication I/O error.
         * Raise CallErrException if there is a server side error and an error
         * grid is returned.
         */
        // Depart from Java - This has a method where mimetype selection can be specified - NOTE: This toolkit
        //    does not yet support a JsonReader
        public HGrid call(string op, HGrid req, string mimeType)
        {
            HGrid res = postGrid(op, req, mimeType);
            if (res.isErr()) throw new CallErrException(res);
            return res;
        }

        public HGrid call(string op, HGrid req)
        {
            HGrid res = postGrid(op, req);
            if (res.isErr()) throw new CallErrException(res);
            return res;
        }

        private HGrid postGrid(string op, HGrid req)
        {
            string reqStr = HZincWriter.gridToString(req);
            string resStr = postString(uri + op, reqStr, null);
            return new HZincReader(resStr).readGrid();
        }

        // Depart from Java - This has a method where mimetype selection can be specified - NOTE: This toolkit
        //    does not yet support a JsonReader
        private HGrid postGrid(string op, HGrid req, string mimeType)
        {
            string reqStr = HZincWriter.gridToString(req);
            string resStr = postString(uri + op, reqStr, mimeType);
            return new HZincReader(resStr).readGrid();
        }

        private string postString(string uriStr, string req)
        {
            return postString(uriStr, req, null);
        }

        private string postString(string uriStr, string req, string mimeType)
        {
            try
            {
                // setup the POST request
                UriBuilder builder = new UriBuilder(uriStr);
                var c = OpenHttpConnection(builder.Uri, "POST");
                c = m_auth.Prepare(c);
                // Depart from Java - This toolkit defaults to zinc instead of plain text.
                c.ContentType = mimeType ?? "text/zinc";
                c.Accept = mimeType ?? "text/zinc";
                byte[] data = Encoding.UTF8.GetBytes(req);
                c.ContentLength = data.Length;
                Stream stream = c.GetRequestStream();
                stream.Write(data, 0, data.Length);
                stream.Close();
                using (var resp = (HttpWebResponse)c.GetResponse())
                {
                    if (resp == null)
                        throw new CallHttpException((int)resp.StatusCode, "Failed to send POST, null response");
                    else if ((int)resp.StatusCode != 200) // Ok
                        throw new CallHttpException((int)resp.StatusCode, resp.StatusDescription);
                    var sr = new StreamReader(resp.GetResponseStream());
                    return sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                throw new CallNetworkException(e);
            }
        }
        #endregion // AddedFromJavaActionsAndCall
        ////////////////////////////////////////////////////////////////
        // Utils
        ////////////////////////////////////////////////////////////////

        private HttpWebRequest OpenHttpConnection(Uri url, string method)
        {
            try
            {
                return OpenHttpConnection(url, method, m_iConnectTimeout, m_iReadTimeout);
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        public static HttpWebRequest OpenHttpConnection(Uri url, string method, int connectTimeout, int readTimeout)
        {
            try
            {
                HttpWebRequest c = (HttpWebRequest)WebRequest.Create(url);
                c.Method = method;
                c.AllowAutoRedirect = false;
                c.Timeout = connectTimeout;
                c.ReadWriteTimeout = readTimeout;
                return c;
            }
            catch (IOException e)
            {
                throw e;
            }
        }

        ///////////////////////////////////////////////////////////////////////
        // Property - not this has been excluded from tidy due to simple nature
        ///////////////////////////////////////////////////////////////////////

        internal class Property
        {
            internal Property(string key, string value)
            {
                this.key = key;
                this.value = value;
            }

            public override string ToString()
            {
                return "[Property " + "key:" + key + ", " + "value:" + value + "]";
            }

            internal readonly string key;
            internal readonly string value;
        }

        ////////////////////////////////////////////////////////////////
        // main
        ////////////////////////////////////////////////////////////////
    
        internal static HClient MakeClient(string uri, string user, string pass)
        {
            // create proper client
            try
            {
                return HClient.Open(uri, user, pass);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public static void Main(string[] args)
        {
            try
            {
                if (args.Length != 3)
                {
                    Console.WriteLine("usage: HClient <uri> <user> <pass>");
                    Environment.Exit(0);
                }

                HClient client = MakeClient(args[0], args[1], args[2]);
                Console.WriteLine(client.GetString("about", new Dictionary<string, string>(), "text/zinc"));
                Console.ReadKey();
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}