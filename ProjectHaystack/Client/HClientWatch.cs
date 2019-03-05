using System;

namespace ProjectHaystack.Client
{
  // NOTE: I don't like this OO design - to me this acts more like a struct
  //  to me the way this works with HClient is it breaks encapsulation (granted they had as an internal
  //  class which I changed)  - might in the future do a OO reorg here.
  public class HClientWatch : HWatch
  {
    private readonly IHClient m_client;
    private readonly string m_dis;

    public HClientWatch(IHClient c, string d, HNum l)
    {
      m_client = c;
      m_dis = d;
      Lease = l;
    }

    public string ID { get; set; }
    public HNum Lease { get; set; }
    public bool Closed { get; set; }

    public override string id()
    {
      return ID;
    }

    public override HNum lease()
    {
      return Lease;
    }

    public override string dis()
    {
      return m_dis;
    }

    public override HGrid sub(HRef[] ids, bool bChecked)
    {
      if (ids.Length == 0)
        throw new ArgumentException("ids are empty", "ids");
      if (Closed)
        throw new InvalidOperationException("watch is closed");

      // grid meta
      HGridBuilder b = new HGridBuilder();
      if (ID != null) b.Meta.add("watchId", ID);
      if (Lease != null) b.Meta.add("lease", Lease);
      b.Meta.add("watchDis", dis());

      // grid rows
      b.addCol("id");
      for (int i = 0; i < ids.Length; ++i)
        b.addRow(new HVal[] { ids[i] });

      // make request
      HGrid res;
      try
      {
        HGrid req = b.toGrid();
        res = m_client.call("watchSub", req);
      }
      catch (CallErrException e)
      {
        // any server side error is considered close
        close(false);
        throw e;
      }

      // make sure watch is stored with its watch id
      if (ID == null)
      {
        ID = res.meta.getStr("watchId");
        Lease = (HNum)res.meta.get("lease");
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

    public override void unsub(HRef[] ids)
    {
      if (ids.Length == 0) throw new ArgumentException("ids are empty", "ids");
      if (ID == null) throw new InvalidOperationException("nothing subscribed yet");
      if (Closed) throw new InvalidOperationException("watch is closed");

      // grid meta
      HGridBuilder b = new HGridBuilder();
      b.Meta.add("watchId", ID);

      // grid rows
      b.addCol("id");
      for (int i = 0; i < ids.Length; ++i)
        b.addRow(new HVal[] { ids[i] });

      // make request
      HGrid req = b.toGrid();
      m_client.call("watchUnsub", req);
    }

    public override HGrid pollChanges()
    {
      return pollChanges(false);
    }

    public HGrid pollChanges(bool bRefresh)
    {
      if (ID == null) throw new InvalidOperationException("nothing subscribed yet");
      if (Closed) throw new InvalidOperationException("watch is closed");

      // grid meta
      HGridBuilder b = new HGridBuilder();
      b.Meta.add("watchId", ID);
      if (bRefresh)
        b.Meta.add("refresh");
      b.addCol("empty");

      // make request
      HGrid req = b.toGrid();
      try
      {
        return m_client.call("watchPoll", req);
      }
      catch (CallErrException e)
      {
        // any server side error is considered close
        close(false);
        throw e;
      }
    }

    public override HGrid pollRefresh()
    {
      return pollChanges(true);
    }

    public override void close()
    {
      close(true);
    }

    public void close(bool bSend)
    {
      // mark flag on watch itself, short circuit if already closed
      if (Closed) return;
      Closed = true;

      // optionally send close message to server
      if (bSend)
      {
        try
        {
          HGridBuilder b = new HGridBuilder();
          b.Meta.add("watchId", ID).add("close");
          b.addCol("id");
          m_client.call("watchUnsub", b.toGrid());
        }
        catch (Exception /*e*/)
        {
          // YET TO DO - we aren't doing anything here, why?
        }
      }
    }

    public override bool isOpen()
    {
      return !Closed;
    }
  }
}