//
// Copyright (c) 2017, SkyFoundry LLC
// (Ian Davies) Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using ProjectHaystack.Auth;
using ProjectHaystack.io;
using ProjectHaystack.Util;

namespace ProjectHaystack.Client
{

  /// <summary>
  /// HAsyncClient asynchronously manages a logical connection to a HTTP REST haystack server.
  /// </summary>
  /// <seealso> cref= <a href='http://project-haystack.org/doc/Rest'>Project Haystack</a> </seealso>
  public class HAsyncClient : IHClient, IHProj
  {
    private readonly AsyncAuthClientContext _context;
    private int _connectTimeout = 60 * 1000;
    private int _readTimeout = 60 * 1000;

    /// <summary>
    /// Constructor with URI to server's API and authentication credentials.
    /// </summary>
    public HAsyncClient(Uri uri, string user, string pass)
      : this(uri, new AsyncAuthClientContext(uri, user, pass))
    { }

    /// <summary>
    /// Constructor with URI to server's API and authentication credentials.
    /// </summary>
    public HAsyncClient(Uri uri, AsyncAuthClientContext context)
    {
      Uri = uri.EndWithSlash();
      _context = context;
    }

    /// <summary>
    /// Base URI for connection such as "http://host/api/demo/".
    /// This Uri always ends with a slash. 
    /// </summary>
    public Uri Uri { get; private set; }

    /// <summary>
    /// Timeout in milliseconds for opening the HTTP socket
    /// </summary>
    public int ConnectTimeout
    {
      get { return _connectTimeout; }
      set
      {
        if (value < 0)
        {
          throw new ArgumentException("Invalid timeout: " + value);
        }
        _connectTimeout = value;
      }
    }

    /// <summary>
    /// Timeout in milliseconds for reading from the HTTP socket
    /// </summary>
    public int ReadTimeout
    {
      get { return _readTimeout; }
      set
      {
        if (value < 0)
        {
          throw new ArgumentException("Invalid timeout: " + value);
        }
        _readTimeout = value;
      }
    }

    public virtual async Task OpenAsync()
    {
      _context.connectTimeout = ConnectTimeout;
      _context.readTimeout = ReadTimeout;
      await _context.OpenAsync();
    }

    #region AddedFromJavaToolkitOperations

    // Added from Java Toolkit
    // Call "about" to query summary info.
    public HDict about()
    {
      return call("about", HGrid.InstanceEmpty, "text/zinc").row(0);
    }

    /**
     * Call "ops" to query which operations are supported by server.
     */
    public HGrid ops()
    {
      return call("ops", HGrid.InstanceEmpty, "text/zinc");
    }

    /**
     * Call "formats" to query which MIME formats are available.
     */
    public HGrid formats()
    {
      return call("formats", HGrid.InstanceEmpty, "text/zinc");
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
    public Task<string> GetStringAsync(string op, Dictionary<string, string> @params, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
    {
      NameValueCollection queryString = HttpUtility.ParseQueryString(String.Empty);
      foreach (KeyValuePair<string, string> x in @params)
      {
        queryString[x.Key] = x.Value;
      }
      return HandleHttpRequestAsync(op + "?" + queryString.ToString(), c =>
      {
        c.ContentType = mimeRequest ?? "text/plain";
        c.Accept = mimeResponse ?? "text/plain";
      });
    }

    /// <summary>
    /// Make a call with the given operation and post to the uri. Response is returned as a string.
    /// </summary>
    /// <param name="op">Given operation</param>
    /// <param name="req">Properly formatted request string</param>
    /// <param name="mimeRequest">Mime type for ContentType header</param>
    /// <param name="mimeResponse">Mime type for Accept header</param>
    /// <returns>Raw string of the result</returns>
    public Task<string> PostStringAsync(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc")
    {
      return HandleHttpRequestAsync(op, c =>
      {
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
      });
    }

    private async Task<string> HandleHttpRequestAsync(string action, Action<HttpWebRequest> requestConfigurator)
    {
      var resp = await _context.ServerCallAsync(action, requestConfigurator);
      using (var reader = new StreamReader(resp.GetResponseStream()))
      {
        return await reader.ReadToEndAsync();
      }
    }

    #region AddedFromJavaRegionReads
    //////////////////////////////////////////////////////////////////////////
    // Reads
    //////////////////////////////////////////////////////////////////////////

    /**
     * Call "read" to lookup an entity record by it's unique identifier.
     * If not found then return null or throw an UnknownRecException based
     * on checked.
     * NOTE: Was final
     */
    public HDict readById(HRef id, bool bChecked)
    {
      HDict rec = readById(id);
      if (rec != null)
        return rec;
      if (bChecked)
        throw new Exception("rec not found for: " + id.ToString());
      return null;
    }

    public HDict readById(HRef id)
    {
      HGrid res = readByIds(new HRef[] { id }, false);
      if (res.isEmpty())
        return null;
      HDict rec = res.row(0);
      if (rec.missing("id"))
        return null;
      return rec;
    }

    /**
     * Read a list of entity records by their unique identifier.
     * Return a grid where each row of the grid maps to the respective
     * id array (indexes line up).  If checked is true and any one of the
     * ids cannot be resolved then raise UnknownRecException for first id
     * not resolved.  If checked is false, then each id not found has a
     * row where every cell is null.
     * NOTE: Was final
     */
    public HGrid readByIds(HRef[] ids, bool bChecked)
    {
      HGrid grid = readByIds(ids);
      if (bChecked)
      {
        for (int i = 0; i < grid.numRows; ++i)
          if (grid.row(i).missing("id"))
            throw new Exception("rec not found for: " + ids[i].ToString());
      }
      return grid;
    }

    public HGrid readByIds(HRef[] ids)
    {
      HGridBuilder b = new HGridBuilder();
      b.addCol("id");
      for (int i = 0; i < ids.Length; ++i)
        b.addRow(new HVal[] { ids[i] });
      HGrid req = b.toGrid();
      return call("read", req, "text/zinc");
    }

    /**
     * Query one entity record that matches the given filter.  If
     * there is more than one record, then it is undefined which one is
     * returned.  If there are no matches than return null or raise
     * UnknownRecException based on checked flag.
     * NOTE: Was final
     */
    public async Task<HDict> readAsync(string filter, bool bChecked)
    {
      HGrid grid = await readAllAsync(filter, 1);
      if (grid.numRows > 0)
        return grid.row(0);
      if (bChecked)
        throw new Exception("rec not found for: " + filter);
      return null;
    }

    public HDict read(string filter, bool bChecked)
    {
      return readAsync(filter, bChecked).Result;
    }

    public Task<HGrid> readAllAsync(string filter, int limit)
    {
      HGridBuilder b = new HGridBuilder();
      b.addCol("filter");
      b.addCol("limit");
      b.addRow(new HVal[] { HStr.make(filter), HNum.make(limit) });
      HGrid req = b.toGrid();
      return CallAsync("read", req, "text/zinc");
    }
    public HGrid readAll(string filter, int limit)
    {
      return readAllAsync(filter, limit).Result;
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
      return call("eval", req, "text/zinc");
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
    public async Task<HGrid[]> EvalAllAsync(HGrid req, bool bChecked)
    {
      string reqStr = HZincWriter.gridToString(req);
      string resStr = await PostStringAsync("evalAll", reqStr, null);
      HGrid[] res = new HZincReader(resStr).readGrids();
      if (bChecked)
      {
        for (int i = 0; i < res.Length; ++i)
          if (res[i].isErr())
            throw new CallErrException(res[i]);
      }
      return res;
    }

    public HGrid[] evalAll(HGrid req, bool bChecked)
    {
      return EvalAllAsync(req, bChecked).Result;
    }

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
      HGrid res = call("pointWrite", req, "text/zinc");
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
      HGrid res = call("pointWrite", req, "text/zinc");
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
    public HGrid hisRead(HRef id, object range)
    {
      HGridBuilder b = new HGridBuilder();
      b.addCol("id");
      b.addCol("range");
      b.addRow(new HVal[] { id, HStr.make(range.ToString()) });
      HGrid req = b.toGrid();
      HGrid res = call("hisRead", req, "text/zinc");
      return res;
    }

    /**
     * Write a set of history time-series data to the given point record.
     * The record must already be defined and must be properly tagged as
     * a historized point.  The timestamp timezone must exactly match the
     * point's configured "tz" tag.  If duplicate or out-of-order items are
     * inserted then they must be gracefully merged.
     */
    public void hisWrite(HRef id, HHisItem[] items)
    {
      HDict meta = new HDictBuilder().add("id", id).toDict();
      HGrid req = HGridBuilder.hisItemsToGrid(meta, items);
      call("hisWrite", req, "text/zinc");
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
      return invokeAction(id, action, args, null);
    }

    // Depart from Java - This has a method where mimetype selection can be specified - NOTE: This toolkit
    //  does not yet support a JsonReader
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
    public async Task<HGrid> CallAsync(string op, HGrid req, string mimeType)
    {
      HGrid res = await PostGridAsync(op, req, mimeType);
      if (res.isErr())
        throw new CallErrException(res);
      return res;
    }

    public HGrid call(string op, HGrid req, string mimeType)
    {
      return CallAsync(op, req, mimeType).Result;
    }

    public HGrid call(string op, HGrid req)
    {
      return call(op, req, null);
    }

    private async Task<HGrid> PostGridAsync(string op, HGrid req, string mimeType)
    {
      string reqStr = HZincWriter.gridToString(req);
      string resStr = await PostStringAsync(op, reqStr, mimeType);
      return new HZincReader(resStr).readGrid();
    }

    #endregion // AddedFromJavaActionsAndCall
  }
}