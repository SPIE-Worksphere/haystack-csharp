//
// Copyright (c) 2018
// Licensed under the Academic Free License version 3.0
//
// History:
//   24 Jun 2018 Ian Davies Creation based on Java Toolkit at same time from project-haystack.org downloads
//
using System;

namespace ProjectHaystack
{
  /**
   * HProj is the common interface for HClient and HServer to provide
   * access to a database tagged entity records.
   *
   * @see <a href='http://project-haystack.org/doc/TagModel'>Project Haystack</a>
   */
  public abstract class HProj : IHProj
  {
    //////////////////////////////////////////////////////////////////////////
    // Operations
    //////////////////////////////////////////////////////////////////////////

    // Get the summary "about" information.
    public abstract HDict about();

    //////////////////////////////////////////////////////////////////////////
    // Read by id
    //////////////////////////////////////////////////////////////////////////

    // Convenience for "readById(id, true)"  NOTE: Was final
    public HDict readById(HRef id)
    {
      return readById(id, true);
    }

    /**
     * Call "read" to lookup an entity record by it's unique identifier.
     * If not found then return null or throw an UnknownRecException based
     * on checked.
     * NOTE: Was final
     */
    public HDict readById(HRef id, bool bChecked)
    {
      HDict rec = onReadById(id);
      if (rec != null)
        return rec;
      if (bChecked)
        throw new Exception("rec not found for: " + id.ToString());
      return null;
    }

    // Convenience for "readByIds(ids, true)" NOTE: Was final
    public HGrid readByIds(HRef[] ids)
    {
      return readByIds(ids, true);
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
      HGrid grid = onReadByIds(ids);
      if (bChecked)
      {
        for (int i = 0; i < grid.numRows; ++i)
          if (grid.row(i).missing("id"))
            throw new Exception("rec not found for: " + ids[i].ToString());
      }
      return grid;
    }

    // Subclass hook for readById, return null if not found.
    protected abstract HDict onReadById(HRef id);

    /**
     * Subclass hook for readByIds, return rows with nulls cells
     * for each id not found.
     */
    protected abstract HGrid onReadByIds(HRef[] ids);

    //////////////////////////////////////////////////////////////////////////
    // Read by filter
    //////////////////////////////////////////////////////////////////////////

    /**
     * Convenience for "read(filter, true)".  NOTE: Was final
     */
    public HDict read(string filter)
    {
      return read(filter, true);
    }

    /**
     * Query one entity record that matches the given filter.  If
     * there is more than one record, then it is undefined which one is
     * returned.  If there are no matches than return null or raise
     * UnknownRecException based on checked flag.
     * NOTE: Was final
     */
    public HDict read(string filter, bool bChecked)
    {
      HGrid grid = readAll(filter, 1);
      if (grid.numRows > 0)
        return grid.row(0);
      if (bChecked)
        throw new Exception("rec not found for: " + filter);
      return null;
    }

    // Convenience for "readAll(filter, max)".  NOTE: Was final
    public HGrid readAll(string filter)
    {
      return readAll(filter, Int32.MaxValue);
    }

    /**
     * Call "read" to query every entity record that matches given filter.
     * Clip number of results by "limit" parameter.
     * NOTE: Was final
     */
    public HGrid readAll(string filter, int limit)
    {
      return onReadAll(filter, limit);
    }

    /**
     * Subclass hook for read and readAll.
     */
    protected abstract HGrid onReadAll(string filter, int limit);

    //////////////////////////////////////////////////////////////////////////
    // Watches
    //////////////////////////////////////////////////////////////////////////

    /**
     * Create a new watch with an empty subscriber list.  The dis
     * string is a debug string to keep track of who created the watch.
     * Pass the desired lease time or null to use default.
     */
    public abstract HWatch watchOpen(String dis, HNum lease);

    /**
     * List the open watches.
     */
    public abstract HWatch[] watches();

    /**
     * Convenience for "watch(id, true)" NOTE: Was final
     */
    public HWatch watch(string id)
    {
      return watch(id, true);
    }

    /**
     * Lookup a watch by its unique identifier.  If not found then
     * raise UnknownWatchErr or return null based on checked flag.
     */
    public abstract HWatch watch(string id, bool bChecked);

    //////////////////////////////////////////////////////////////////////////
    // Historian
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
    public abstract HGrid hisRead(HRef id, object range);

    /**
     * Write a set of history time-series data to the given point record.
     * The record must already be defined and must be properly tagged as
     * a historized point.  The timestamp timezone must exactly match the
     * point's configured "tz" tag.  If duplicate or out-of-order items are
     * inserted then they must be gracefully merged.
     */
    public abstract void hisWrite(HRef id, HHisItem[] items);
  }
}
