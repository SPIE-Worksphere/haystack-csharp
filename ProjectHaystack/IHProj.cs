namespace ProjectHaystack
{
  public interface IHProj
  {
    HDict about();
    HGrid hisRead(HRef id, object range);
    void hisWrite(HRef id, HHisItem[] items);
    HGrid readAll(string filter, int limit);
    HDict readById(HRef id, bool bChecked);
    HGrid readByIds(HRef[] ids, bool bChecked);
  }
}