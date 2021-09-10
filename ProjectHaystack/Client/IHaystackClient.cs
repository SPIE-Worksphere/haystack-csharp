using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjectHaystack.Client
{
    /// <summary>
    /// Interface for ProjectHaystack clients.
    /// </summary>
    public interface IHaystackClient
    {
        #region Inspection

        /// <summary>
        /// Get server summary using the "about" call.
        /// </summary>
        Task<HaystackDictionary> AboutAsync();

        /// <summary>
        /// Get list of allowed server operations using the "ops" call.
        /// </summary>
        Task<HaystackGrid> OpsAsync();

        /// <summary>
        /// Get list of MIME formats using the "formats" call.
        /// </summary>
        Task<HaystackGrid> FormatsAsync();

        #endregion Inspection

        #region Raw calls

        /// <summary>
        /// Execute a GET request and return the raw string result.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="params">Dictionary containing query parameters.</param>
        /// <param name="mimeResponse">MIME type requested for the response.</param>
        /// <returns>Raw HTTP content.</returns>
        Task<string> GetStringAsync(string op, Dictionary<string, string> @params, string mimeResponse = "text/zinc");

        /// <summary>
        /// Execute a POST request and return the raw string result.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Properly formatted request string</param>
        /// <param name="mimeRequest">MIME type of the request.</param>
        /// <param name="mimeResponse">MIME type requested for the response.</param>
        /// <returns>Raw HTTP content.</returns>
        Task<string> PostStringAsync(string op, string req, string mimeRequest = "text/zinc", string mimeResponse = "text/zinc");

        /// <summary>
        /// Invoke an action using the "invokeAction" call.
        /// </summary>
        /// <param name="id">Target to invoke the action on.</param>
        /// <param name="action">Action to invoke.</param>
        /// <param name="args">Action arguments.</param>
        /// <returns>Action result grid.</returns>
        Task<HaystackGrid> InvokeActionAsync(HaystackReference id, string action, HaystackDictionary args);

        #endregion Raw calls

        #region Reads

        /// <summary>
        /// Read all records with a given filter.
        /// </summary>
        /// <param name="filter">Record filter.</param>
        /// <param name="limit">Maximum number of results to request.</param>
        /// <returns>Grid with records.</returns>
        Task<HaystackGrid> ReadAllAsync(string filter, int limit);

        /// <summary>
        /// Read any one record that matches a given filter.
        /// If no records apply an exception is thrown.
        /// </summary>
        /// <param name="filter">Record filter.</param>
        /// <returns>Matching record.</returns>
        Task<HaystackDictionary> ReadAsync(string filter);

        /// <summary>
        /// Read all records by their given unique ID's.
        /// Throws an exception if any record was not found.
        /// </summary>
        /// <param name="ids">List of record ID's.</param>
        /// <returns>Grid with records.</returns>
        Task<HaystackGrid> ReadByIdsAsync(HaystackReference[] ids);

        /// <summary>
        /// Read a single record by its unique ID.
        /// Throws an exception if the record was not found.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <returns>Matching record.</returns>
        Task<HaystackDictionary> ReadByIdAsync(HaystackReference id);

        #endregion Reads

        #region Evals

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="expr">Expression.</param>
        /// <returns>Grid of results.</returns>
        Task<HaystackGrid> EvalAsync(string expr);

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="exprs">List of expressions.</param>
        /// <returns>Grid of results per expression.</returns>
        Task<HaystackGrid[]> EvalAllAsync(HaystackGrid req);

        /// <summary>
        /// Use vendor specific logic using the "eval" call.
        /// </summary>
        /// <param name="req">Grid with expressions in the "expr" field.</param>
        /// <returns>Grid of results per expression.</returns>
        Task<HaystackGrid[]> EvalAllAsync(string[] exprs);

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
        Task<HaystackGrid> HisReadAsync(HaystackReference id, HaystackDateTimeRange range);

        /// <summary>
        /// Read history time-series data for a given point record and time range.
        /// The range has an inclusive start and an exclusive end.
        /// The range must match the timezone configured on the history record.
        /// The range will use the timezone of the record.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="range">Time range.</param>
        /// <returns>Grid of time-series data.</returns>
        Task<HaystackGrid> HisReadAsync(HaystackReference id, string range);

        /// <summary>
        /// Write a set of history time-series data to a given point record.
        /// The record must already exist and tagged as a historized point.
        /// The timestamp timezone must exactly match the point's timezone "tz" tag.
        /// </summary>
        /// <param name="id">Record ID.</param>
        /// <param name="items">Time-series data.</param>
        /// <param name="metaData">Optional metadata to include.</param>
        Task<HaystackGrid> HisWriteAsync(HaystackReference id, HaystackHistoryItem[] items, HaystackDictionary metaData = null);

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
        Task<HaystackGrid> PointWriteAsync(HaystackReference id, int level, string who, HaystackValue val, HaystackNumber dur);

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
        Task<HaystackGrid> PointWriteArrayAsync(HaystackReference id);

        #endregion Points

        #region Core calls

        /// <summary>
        /// Call the given operation and throw server-side exceptions.
        /// </summary>
        /// <param name="op">Operation to execute.</param>
        /// <param name="req">Request content.</param>
        /// <returns>Grid of results.</returns>
        Task<HaystackGrid> CallAsync(string op, HaystackGrid req);

        #endregion Core calls
    }
}